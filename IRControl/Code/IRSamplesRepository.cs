using System;
using System.Collections.Generic;
using System.Linq;
using IRControl.Models;

namespace IRControl.Code
{
    public class IRSamplesRepository
    {
        IRSamplesContext db = new IRSamplesContext();

        public void AddSample(IRSample sample)
        {
            db.IRSamples.Add(sample);
            db.SaveChanges();
        }
        public int GetStoredSamplesCount()
        {
            return db.IRSamples.Count();
        }

        public IEnumerable<IRSample> GetLastSamples(int count)
        {
            IEnumerable<IRSample> samples = db.IRSamples
                .OrderByDescending(x => x.Id)
                .Take(count);

            return samples.OrderBy(x => x.Id);
        }

        public void DeleteSample(int id)
        {
            var sample = db.IRSamples.FirstOrDefault(x => x.Id == id);

            if (sample != null)
            {
                db.IRSamples.Remove(sample);
                db.SaveChanges();
            }
        }

        public IRSample GetSample(int id)
        {
            var sample = db.IRSamples.FirstOrDefault(x => x.Id == id);

            return sample;
        }

    }
}