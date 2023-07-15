using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Entities
{
    public class YearPlan
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public Type Type { get; set; }
        public int Plan { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        public YearPlan()
        {

        }
        public YearPlan(int id, int year, int plan, DateTime created, DateTime lastUpdated)
        {
            Id = id;
            Year = year;
            Plan = plan;
            Created = created;
            LastUpdated = lastUpdated;
        }
    }
}
