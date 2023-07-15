using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Service
{
    public class YearPlanService
    {
        private YearPlanDAO _yearPlanDAO;

        public YearPlanService(YearPlanDAO yearPlanDAO)
        {
            _yearPlanDAO = yearPlanDAO;
        }
        public List<YearPlan> GetByYearAndMo(int year, int mosId)
        {
            return _yearPlanDAO.FindByYearAndMo(year, mosId);
        }

        public YearPlan Create(YearPlan yearPlan, int mosId)
        {
            return _yearPlanDAO.Create(yearPlan, mosId);
        }

        public YearPlan Update(YearPlan yearPlan, int mosId)
        {
            return _yearPlanDAO.Update(yearPlan, mosId);
        }

        public bool Delete(int yearPlanId)
        {
            return _yearPlanDAO.Delete(yearPlanId);
        }
    }
}
