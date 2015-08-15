using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire;
using Hangfire.Storage;
using Temperature.Models;

namespace Temperature.Code
{
    public class TemperatureTrackingService
    {
        TemperatureRepository repository;
        ArduinoTemperatureSensor sensor;

        public TemperatureTrackingService()
            :this(new TemperatureRepository(), new ArduinoTemperatureSensor())
        {
        }

        public TemperatureTrackingService(TemperatureRepository repository, 
            ArduinoTemperatureSensor sensor)
        {
            this.repository = repository;
            this.sensor = sensor;
        }

        public  void StartService()
        {
            RecurringJob.AddOrUpdate("StoreTemperatureToDB", () => StoreTemperatureToDB(), 
                Cron.Minutely, TimeZoneInfo.Local);
        }

        public  void StopService()
        {
            RecurringJob.RemoveIfExists("StoreTemperatureToDB");

        }

        public  bool IsServiceRunning()
        {
            try
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var recurring = connection.GetRecurringJobs()
                        .FirstOrDefault(p => p.Id == "StoreTemperatureToDB");

                    if (recurring == null || !recurring.NextExecution.HasValue)
                        return false;
                    else
                        return true;
                }
            }
            catch {return false;}
        }

        public  DateTime? GetNextTrackingTime()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var recurring = connection.GetRecurringJobs()
                    .FirstOrDefault(p => p.Id == "StoreTemperatureToDB");

                if (recurring == null || !recurring.NextExecution.HasValue)
                    return null;
                else
                    return recurring.NextExecution.Value.ToLocalTime();
            }
        }

        public  void StoreTemperatureToDB()
        {
            TemperatureSample sample = sensor.GetTemperatureFromArduino();

            if (sample == null)
                return;

            repository.AddSample(sample);
        }
    }
}