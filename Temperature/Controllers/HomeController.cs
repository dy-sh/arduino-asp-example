using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Temperature.Code;
using Temperature.Models;

namespace Temperature.Controllers
{
    public class HomeController : Controller
    {
        ArduinoTemperatureSensor sensor = new ArduinoTemperatureSensor();
        TemperatureRepository repository = new TemperatureRepository();
        TemperatureTrackingService trackingService;

        public HomeController()
        {
            trackingService = new TemperatureTrackingService(repository, sensor);

            //Change JSON separator (,) to (.)
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;
        }


        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult TemperatureChart()
        {
            IEnumerable<TemperatureSample> temperatureSamples =
                repository.GetLastSamples(100);

            // return PartialView("_TemperatureChart", temperatureSamples);
            return PartialView("_TemperatureChart");
        }

        public ActionResult TemperaturePanel()
        {
            return PartialView("_TemperaturePanel");
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Last days</param>
        /// <returns></returns>
        public JsonResult GetAvarageByHourSamples(int id = 1)
        {
            var samples = repository.GetAvarageByHourSamples(id);

            if (samples == null) return null;

            var dateTime = new List<string>();
            var temperature = new List<string>();
            var humidity = new List<string>();

            //  Random rnd = new Random();
            foreach (var item in samples)
            {
                dateTime.Add(String.Format("{0:yyyy-MM-dd HH:mm:ss}", item.DateTime));
                //    temperature.Add((item.Temperature + rnd.Next(-3, 3)).ToString());
                temperature.Add(item.Temperature == null ? null : ((float)item.Temperature).ToString("0.0"));
                humidity.Add(item.Humidity == null ? null : ((float)item.Humidity).ToString("0.0"));
            }

            return Json(new { dateTime, temperature, humidity }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCurrentTemperature()
        {
            TemperatureSample sample =
                sensor.GetTemperatureFromArduino();

            if (sample == null) return null;

            var dateTime = String.Format("{0:yyyy-MM-dd HH:mm:ss}", sample.DateTime);
            int temperature =  (int)sample.Temperature;
            int humidity = (int)sample.Humidity;
 
            return Json(new { dateTime, temperature, humidity }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Start()
        {
            trackingService.StartService();
            return RedirectToAction("Settings");
        }

        public ActionResult Stop()
        {
            trackingService.StopService();
            return RedirectToAction("Settings");
        }

        public ActionResult Settings()
        {
            Response.AppendHeader("Refresh", "10");

            ViewBag.IsServerOnline = sensor.IsArduinoOnline();
            if (ViewBag.IsServerOnline)
            {
                ServerStatus serverStatus =
                    sensor.GetServerStatusFromArduino();

                ViewBag.OnlineTime = serverStatus.TimeOnline;
                ViewBag.TotalRequests = serverStatus.TotalRequests;
            }

            ViewBag.IsTrackingRunning = trackingService.IsServiceRunning();
            if (ViewBag.IsTrackingRunning)
            {
                ViewBag.NextTrackingTime = trackingService.GetNextTrackingTime();
                ViewBag.NextTrackingIn = (int)((TimeSpan)(ViewBag.NextTrackingTime - DateTime.Now)).TotalSeconds;
            }

            ViewBag.SamplesCount = repository.GetStoredSamplesCount();


            return View();
        }
    }
}