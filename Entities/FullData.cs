using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Entities
{
    public class FullData
    {
        public int? Id { get; set; }
        public Mo MoWithAttachedPopulation { get; set; }
        public Mo MoPerformedMammography { get; set; }
        public List<Mo> MosForWhichMammographyWasPerformed { get; set; }
        public bool IsMammographAvaliable { get; set; }
        public bool IsMammographWasWorking { get; set; }
        public YearPlan YearPlan { get; set; }
        public Week Week { get; set; }
        public Type Type { get; set; }
        public int NumberOfPerformedMammography { get; set; }
        public int NumberOfPerformedMammographyByYear { get; set; }
        public int NumberOfDetectedPathology { get; set; }
        public int NumberOfDetectedPathologyByYear { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }

        public FullData()
        {
        }

        public FullData(int id, Mo moWithAttachedPopulation, Mo moPerformedMammography, bool isMammographAvaliable, bool isMammographWasWorking, YearPlan yearPlan, Week week, Type type, int numberOfPerformedMammography, int numberOfDetectedPathology, string comment, DateTime created, DateTime lastUpdated)
        {
            Id = id;
            MoWithAttachedPopulation = moWithAttachedPopulation;
            MoPerformedMammography = moPerformedMammography;
            IsMammographAvaliable = isMammographAvaliable;
            IsMammographWasWorking = isMammographWasWorking;
            this.YearPlan = yearPlan;
            this.Week = week;
            this.Type = type;
            NumberOfPerformedMammography = numberOfPerformedMammography;
            NumberOfDetectedPathology = numberOfDetectedPathology;
            Comment = comment;
            Created = created;
            LastUpdated = lastUpdated;
        }
    }
}
