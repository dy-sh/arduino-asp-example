using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Web;
using Hangfire;
using Hangfire.Storage;
using Newtonsoft.Json;
using Temperature.Models;

namespace Temperature.Code
{
    public class TemperatureRepository
    {
        TemperatureSampleContext db = new TemperatureSampleContext();

        #region Utils

        private DateTime BackInTimeToZeroMinutes(DateTime dateTime)
        {
            DateTime proceedDate = new DateTime(
              dateTime.Year,
              dateTime.Month,
              dateTime.Day,
              dateTime.Hour, 0, 0
              );

            return proceedDate;
        }

        #endregion

        public void AddSample(TemperatureSample temperatureSample)
        {
            db.TemperatureSamples.Add(temperatureSample);
            db.SaveChanges();
        }
        public int GetStoredSamplesCount()
        {
            return db.TemperatureSamples.Count();
        }

        public IEnumerable<TemperatureSample> GetLastSamples(int count)
        {
            IEnumerable<TemperatureSample> temperatureSamples = db.TemperatureSamples
                .OrderByDescending(x => x.Id)
                .Take(count);

            return temperatureSamples.OrderBy(x => x.Id);
        }

        public IEnumerable<TemperatureSample> GetSamples(DateTime startTime, DateTime endTime)
        {
            var samples = (from cnt in db.TemperatureSamples
                           where cnt.DateTime >= startTime && cnt.DateTime < endTime
                           select cnt).ToList();

            return samples;
        }


        public IEnumerable<TemperatureSample> GetAvarageByHourSamples(int lastDays)
        {
            return GetAvarageByHourSamples(DateTime.Now.AddDays(-lastDays), DateTime.Now.AddHours(1));
        }

        public IEnumerable<TemperatureSample> GetAvarageByHourSamples(DateTime startTime, DateTime endTime)
        {
            startTime = BackInTimeToZeroMinutes(startTime);
            endTime = BackInTimeToZeroMinutes(endTime);

            var allData = GetSamples(startTime, endTime);

            //allData = allData.Where((x, i) => i % 50 == 0);
            IEnumerable<TemperatureSample> avarageData =
                ConvertSamplesToAvarageByHours(allData);

            return avarageData;
        }


        private IEnumerable<TemperatureSample> ConvertSamplesToAvarageByHours(IEnumerable<TemperatureSample> samples)
        {
            List<TemperatureSample> avarageList = new List<TemperatureSample>();
            List<TemperatureSample> list = samples.OrderBy(x => x.DateTime).ToList();

            TimeSpan totalTimeSpan = list[list.Count-1].DateTime - list[0].DateTime;

            for (int i = 0; i <= (int)totalTimeSpan.TotalHours+1; i++)
            {
                DateTime proceedDate = BackInTimeToZeroMinutes(list[0].DateTime.AddHours(i));

                List<TemperatureSample> proceedList = (from x in list
                            where  
                                x.DateTime >= proceedDate &&
                                x.DateTime < proceedDate.AddHours(1)
                            select x).ToList();

                float? averageTemperature = proceedList.Average(x => x.Temperature);
                float? averageHumidity = proceedList.Average(x => x.Humidity);

                if (averageTemperature!=null)
                avarageList.Add(new TemperatureSample
                {
                    DateTime = proceedDate,
                    Temperature = averageTemperature,
                    Humidity = averageHumidity,
                    Id = i
                });
            }
            return avarageList;
        }


    }


   
}