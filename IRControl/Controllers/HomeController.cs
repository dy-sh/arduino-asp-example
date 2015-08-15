using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IRControl.Code;
using IRControl.Models;

namespace IRControl.Controllers
{
    public class HomeController : Controller
    {
        ArduinoIRControl irControl = new ArduinoIRControl();
        IRSamplesRepository repository = new IRSamplesRepository();

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [HttpGet]
        public ActionResult NewCode()
        {
            IRStatus irStatus = irControl.GetIRStatus();

            ViewBag.waitForIRCode = irStatus.waitForIRCode;
            ViewBag.irCodeStored = irStatus.irCodeStored;

            if (irStatus.waitForIRCode)
                Response.AppendHeader("Refresh", "1");


            if (irStatus.irCodeStored)
            {
                IRSample sample = irControl.GetIRSample();
                return View(sample);
            }
            return View();
        }

        [HttpPost]
        public ActionResult NewCode(IRSample sample)
        {
            if (sample.Description == null)
                sample.Description = sample.Value.ToString();
            if (ModelState.IsValid)
            {
                repository.AddSample(sample);
            }

            return RedirectToAction("NewCode");
        }

        public ActionResult ReadNewSample()
        {
            irControl.ReadNewSample();
            return RedirectToAction("NewCode");
        }

        public ActionResult CancelReadNewSample()
        {
            irControl.CancelReadNewSample();
            return RedirectToAction("NewCode");
        }

        public ActionResult List()
        {
            var samples = repository.GetLastSamples(1000);
            return View(samples);
        }

        public ActionResult Send(int id)
        {
            var sample = repository.GetSample(id);
            if (sample != null)
                irControl.SendIR(sample.Type, sample.Length, sample.Value);
            return RedirectToAction("List");
        }

        public ActionResult Delete(int id)
        {
            repository.DeleteSample(id);

            return RedirectToAction("List");
        }

        public ActionResult Settings()
        {

            return View();
        }
    }
}