using System;
using IRControl.Models;
using Newtonsoft.Json;

namespace IRControl.Code
{
    public class ArduinoIRControl
    {
        private string arduinoAddress = "http://192.168.1.20";

        public bool ReadNewSample(int attemptsToConnect = 10)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress + "/ir/read");
                    IRStatus newSample = JsonConvert.DeserializeObject<IRStatus>(jsonString);

                    return newSample.waitForIRCode == true;
                }
                catch { }

            return false;
        }

        public bool CancelReadNewSample(int attemptsToConnect = 10)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress + "/ir/clear");
                    IRStatus newSample = JsonConvert.DeserializeObject<IRStatus>(jsonString);

                    return newSample.waitForIRCode == false;
                }
                catch { }

            return false;
        }

        public bool IsNewSampleReaded(int attemptsToConnect = 10)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress + "/ir/info");
                    IRStatus newSample = JsonConvert.DeserializeObject<IRStatus>(jsonString);


                    return newSample.irCodeStored;
                }
                catch { }

            return false;
        }

        public bool IsWaitingForSample(int attemptsToConnect = 10)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress + "/ir/info");
                    IRStatus newSample = JsonConvert.DeserializeObject<IRStatus>(jsonString);

                    return newSample.waitForIRCode;
                }
                catch { }

            return false;
        }

        public IRSample GetIRSample(int attemptsToConnect = 10)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress + "/ir/info");
                    IRSample newSample = JsonConvert.DeserializeObject<IRSample>(jsonString);
                    if (newSample.Length > 0)
                        CancelReadNewSample();

                    return newSample;
                }
                catch { }

            return null;
        }

        public IRStatus GetIRStatus(int attemptsToConnect = 10)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress + "/ir/info");
                    IRStatus newSample = JsonConvert.DeserializeObject<IRStatus>(jsonString);

                    return newSample;
                }
                catch { }

            return null;
        }

        public bool SendIR(int type, int length, int value, int attemptsToConnect = 10)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(
                        arduinoAddress
                        + "/ir/send"
                        + "/" + type
                        + "/" + length
                        + "/" + value
                        );
                    // IRStatus newSample = JsonConvert.DeserializeObject<IRStatus>(jsonString);

                    return true;
                }
                catch { }

            return false;
        }

        public ServerStatus GetServerStatus(int attemptsToConnect = 1)
        {
            for (int i = 1; i <= attemptsToConnect; i++)
                try
                {
                    string jsonString = new TimeoutedWebClient().DownloadString(arduinoAddress + "/status");
                    ServerStatus newServerStatus = JsonConvert.DeserializeObject<ServerStatus>(jsonString);
                    return newServerStatus;
                }
                catch { }

            return null;
        }


        public bool IsOnline(int attemptsToConnect = 1)
        {
            if (GetServerStatus(attemptsToConnect) != null)
                return true;
            else
                return false;
        }
    }
}