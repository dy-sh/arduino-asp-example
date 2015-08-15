using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using Antlr.Runtime;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Newtonsoft.Json;
using Temperature.Models;

namespace Temperature.Code
{
    public class ArduinoTemperatureSensor
    {
        private string arduinoAddress = "http://192.168.1.20";
        
        public TemperatureSample GetTemperatureFromArduino(int attemptsToConnect=10)
        {
            for (int i = 1; i <=attemptsToConnect; i++)
            try
            {
                string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress+"/temp");
                TemperatureSample newSample = JsonConvert.DeserializeObject<TemperatureSample>(jsonString);
                newSample.DateTime = DateTime.Now;
                return newSample;
            }
            catch{}

            return null;
        }

        public ServerStatus GetServerStatusFromArduino(int attemptsToConnect = 1)
        {
            for (int i = 1; i <=attemptsToConnect; i++)
            try
            {
                string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress+"/status");
                ServerStatus newServerStatus = JsonConvert.DeserializeObject<ServerStatus>(jsonString);
                return newServerStatus;
            }
            catch{}

            return null;
        }


        public bool IsArduinoOnline(int attemptsToConnect = 1)
        {
            if (GetServerStatusFromArduino(attemptsToConnect) != null)
                return true;
            else
                return false;
        }
    }
}