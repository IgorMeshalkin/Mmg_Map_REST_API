using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Service
{
    public class WeekService
    {
        private WeekDAO _weekDAO;

        public WeekService(WeekDAO weekDAO)
        {
            _weekDAO = weekDAO;
        }

        public List<Week> GetByYear(int year)
        {
            return _weekDAO.FindByYear(year);
        }

        public Week GetById(int Id)
        {
            return _weekDAO.FindById(Id);
        }

        public List<int> GetYears()
        {
            return _weekDAO.FindYears();
        }

        public Week GetLastWeek()
        {
            List<Week> weekByCurrentYear = _weekDAO.FindByYear(DateTime.Now.Year);

            int maxNumber = 0;
            weekByCurrentYear.ForEach(w =>
            {
                if (maxNumber < w.Id)
                {
                    maxNumber = w.Id;
                }
            });

            return weekByCurrentYear.Find(w => w.Id == maxNumber);
        }

        public Week Create(Week week)
        {
            return _weekDAO.Create(week);
        }

        public Week Update(Week week)
        {
            return _weekDAO.Update(week);
        }

        public bool Delete(int weekId)
        {
            return _weekDAO.Delete(weekId);
        }
    }
}
