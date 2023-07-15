using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Service
{
    public class MoService
    {
        private readonly MoDAO _moDAO;
        public MoService(MoDAO moDAO)
        {
            this._moDAO = moDAO;
        }

        public List<Mo> GetAll()
        {
            return _moDAO.FindAll();
        }

        public List<Mo> GetByUserId(int userId)
        {
            return _moDAO.FindByUserId(userId);
        }

        public Mo GetById(int Id)
        {
            return _moDAO.FindById(Id);
        }

        public Mo GetByName(string name)
        {
            return _moDAO.FindByName(name);
        }

        public Mo Create(Mo mo)
        {
            return _moDAO.Create(mo);
        }

        public Mo Update(Mo mo)
        {
            return _moDAO.Update(mo);
        }
    }
}
