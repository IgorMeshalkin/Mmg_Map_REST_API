using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Entities
{
    public class PartialData
    {
        public int Id { get; set; }
        public int YearPlan { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public decimal PercentageOfCompletion { get; set; }

        public PartialData(int id, int yearPlan, int year, int weekNumber)
        {
            Id = id;
            YearPlan = yearPlan;
            Year = year;
            WeekNumber = weekNumber;
        }
    }
}
