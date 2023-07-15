using MmgMapAPI.DAO;
using MmgMapAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.Service
{
    public class UserService
    {
        private UserDAO _userDAO;
        private MoDAO _moDAO;

        public UserService(UserDAO userDAO, MoDAO moDAO)
        {
            _userDAO = userDAO;
            _moDAO = moDAO;
        }

        public List<User> GetAll()
        {
            List<User> result = _userDAO.FindAll();

            result.ForEach(user =>
            {
                if (user.Role == Role.ADMIN)
                {
                    user.Mos = new List<Mo>();
                }
                else
                {
                    user.Mos = _moDAO.FindByUserId(user.Id);
                }
            });

            return result;
        }

        public User GetById(int Id)
        {
            User result = _userDAO.FindById(Id);

            if (result.Role == Role.ADMIN)
            {
                result.Mos = new List<Mo>();
            }
            else
            {
                result.Mos = _moDAO.FindByUserId(result.Id);
            }

            return _userDAO.FindById(Id);
        }

        public User GetByUsername(string username)
        {
            User result = _userDAO.FindByUsername(username);

            if (result != null)
            {
                if (result.Role == Role.ADMIN)
                {
                    result.Mos = new List<Mo>();
                }
                else
                {
                    result.Mos = _moDAO.FindByUserId(result.Id);
                }
            }

            return result;
        }

        public User Create(User user)
        {
            User result = _userDAO.Create(user);
            if (user.Mos.Count > 0)
            {
                _moDAO.AddMosToUser(user.Mos, result.Id);
            }
            return result;
        }

        public User Update(User user)
        {
            User userFromDB = _userDAO.FindById(user.Id);
            User result = _userDAO.Update(user);

            if (userFromDB.Role == Role.USER && user.Role == Role.ADMIN)
            {
                _moDAO.RemoveMosFromUser(user.Mos, user.Id);
            }
            else
            {
                List<Mo> mosFromDB = _moDAO.FindByUserId(user.Id);

                //добавляю МО в список доступных (при необходимости)
                if (user.Mos.Count > 0)
                {
                    List<Mo> mosForAdd = new List<Mo>();

                    user.Mos.ForEach(mo =>
                    {
                        if (!mosFromDB.Contains(mo))
                        {
                            mosForAdd.Add(mo);
                        }
                    });

                    if (mosForAdd.Count > 0)
                    {
                        _moDAO.AddMosToUser(mosForAdd, user.Id);
                    }
                }

                //удаляю МО из списка доступных (при необходимости)
                if (mosFromDB.Count > 0)
                {
                    List<Mo> mosForRemove = new List<Mo>();

                    mosFromDB.ForEach(mo =>
                    {
                        if (!user.Mos.Contains(mo))
                        {
                            mosForRemove.Add(mo);
                        }
                    });

                    if (mosForRemove.Count > 0)
                    {
                        _moDAO.RemoveMosFromUser(mosForRemove, user.Id);
                    }
                }
            }         

            return result;
        }
    }
}

