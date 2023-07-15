using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Service
{
    public class DataService
    {
        private DataDAO _dataDAO;
        private MoDAO _moDAO;

        public DataService(DataDAO dataDAO, MoDAO moDAO)
        {
            this._dataDAO = dataDAO;
            this._moDAO = moDAO;
        }

        public List<FullData> GetFullDataByMoAndWeek(int mosId, int weekId)
        {
            return _dataDAO.FindFullDataByMoAndWeek(mosId, weekId);
        }

        public PartialData GetPartialDataByMoAndWeek(int mosId, int weekId)
        {
            return _dataDAO.FindPartialData(mosId, weekId);
        }

        public FullData Create(FullData data, int mosId, int weekId)
        {
            FullData result = _dataDAO.Create(data, mosId, weekId);

            if (data.Type.Id == 2)
            {
                data.MosForWhichMammographyWasPerformed.ForEach(mo =>
                    {
                        _moDAO.AddMosToData(data.MosForWhichMammographyWasPerformed, (int)result.Id);
                    });
            }

            return result;
        }

        public FullData Update(FullData data)
        {
            FullData result = _dataDAO.Update(data);

            if (data.Type.Id == 2)
            {
                List<Mo> mosFromDB = _moDAO.FindByDataId((int)data.Id);

                //добавляю МО в список МО для которых проводится маммография при работе по самостоятельному тарифу (при необходимости)
                if (data.MosForWhichMammographyWasPerformed.Count > 0)
                {
                    List<Mo> mosForAdd = new List<Mo>();

                    data.MosForWhichMammographyWasPerformed.ForEach(mo =>
                        {
                            if (!mosFromDB.Contains(mo))
                            {
                                mosForAdd.Add(mo);
                            }
                        });

                    if (mosForAdd.Count > 0)
                    {
                        _moDAO.AddMosToData(mosForAdd, (int)data.Id);
                    }
                }

                //удаляю МО из списка МО для которых проводится маммография при работе по самостоятельному тарифу (при необходимости)
                if (mosFromDB.Count > 0)
                {
                    List<Mo> mosForRemove = new List<Mo>();

                    mosFromDB.ForEach(mo =>
                    {
                        if (!data.MosForWhichMammographyWasPerformed.Contains(mo))
                        {
                            mosForRemove.Add(mo);
                        }
                    });

                    if (mosForRemove.Count > 0)
                    {
                        _moDAO.RemoveMosFromData(mosForRemove, (int)data.Id);
                    }
                }
            }

            return result;
        }
    }
}
