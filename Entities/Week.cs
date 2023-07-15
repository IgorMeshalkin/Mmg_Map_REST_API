using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Entities
{
    public class Week
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int NumberInYear { get; set; }
        public decimal Target { get; set; }
        public DateTime FirstDay { get; set; }
        public DateTime EndDay { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        public Week()
        {
        }
        public Week(int id, int year, int numberInYear, decimal target, DateTime firstDay, DateTime endDay, DateTime created, DateTime lastUpdated)
        {
            Id = id;
            Year = year;
            NumberInYear = numberInYear;
            Target = target;
            FirstDay = firstDay;
            EndDay = endDay;
            Created = created;
            LastUpdated = lastUpdated;
        }
    }
}
