using Microsoft.Extensions.Configuration;
using MmgMapAPI.Entities;
using MmgMapAPI.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.DAO
{
    public class UserDAO
    {
        private readonly string CONNECTION_STRING = ConnectionManager.GetConnectionString();

        #region Основные методы
        //Возвращает всех юзеров из БД
        public List<User> FindAll()
        {
            List<User> result = new List<User>();
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"SELECT * FROM \"Users\"";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                result.Add(GetUserFromReader(rdr));
                            }
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return result;
        }

        //Возвращает User по Id или null если такого User нет.
        public User FindById(int Id)
        {
            User result = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                try
                {
                    result = FindById(conn, Id);
                }
                catch
                {
                    return null;
                }
            }
            return result;
        }

        //Возвращает User по Username или null если такого User нет.
        public User FindByUsername(string username)
        {
            User result = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                try
                {
                    result = FindByUsername(conn, username);
                }
                catch
                {
                    return null;
                }
            }
            return result;
        }

        //Сохраняет User в БД и возвращает его с заполненными полями Id, Created и LastUpdated.
        public User Create(User user)
        {
            User result = user;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"INSERT INTO \"Users\" (\"Username\", \"Role\", \"isActive\", \"Created\", \"LastUpdated\") VALUES (@username, @role, @isActive, current_timestamp, current_timestamp)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("username", user.Username);
                    cmd.Parameters.AddWithValue("role", user.Role.ToString());
                    cmd.Parameters.AddWithValue("isActive", user.isActive);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        return null;
                    }
                }

                result = FindByUsername(conn, user.Username);

            }
            return result;
        }

        //Обновляет в БД поля Username, Role и isActive переданного в качестве аргумента User.
        public User Update(User user)
        {
            User result = user;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"UPDATE \"Users\" SET \"Username\" = @username, \"Role\" = @role, \"isActive\" = @isActive, \"LastUpdated\" = current_timestamp WHERE \"Id\" = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("username", user.Username);
                    cmd.Parameters.AddWithValue("role", user.Role.ToString());
                    cmd.Parameters.AddWithValue("isActive", user.isActive);
                    cmd.Parameters.AddWithValue("id", user.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = FindById(conn, user.Id);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Вспомогательные методы
        //Возвращает User по Username или null если такого User нет.
        //Принимает NpgsqlConnection в качестве аргумента (что бы не открывать лишние соединения) и используется в методах Update(User user) и FindById(int Id).
        private User FindById(NpgsqlConnection conn, int id)
        {
            User result = null;

            string commandText = $"SELECT * FROM \"Users\" WHERE \"Id\"= @id";
            using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.Parameters.AddWithValue("id", id);

                //Я не использую try catch для следующего обращения к БД т.к. метод вспомогательный, а исключения обработаны в вызывающих методах
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    result = GetUserFromReader(rdr);
                }
            }
            return result;
        }

        //Возвращает User по Username или null если такого User нет.
        //Принимает NpgsqlConnection в качестве аргумента (что бы не открывать лишние соединения) и используется в методах Create(User user) и FindByUsername(string username).
        private User FindByUsername(NpgsqlConnection conn, string username)
        {
            User result = null;

            string commandText = $"SELECT * FROM \"Users\" WHERE \"Username\"= @username";
            using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.Parameters.AddWithValue("username", username);

                //Я не использую try catch для следующего обращения к БД т.к. метод вспомогательный, а исключения обработаны в вызывающих методах
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    result = GetUserFromReader(rdr);
                }
            }
            return result;
        }

        //Возвращает объект User из объекта NpgsqlDataReader
        private User GetUserFromReader(NpgsqlDataReader rdr)
        {
            User result = new User((int)rdr["Id"]
                , (string)rdr["Username"]
                , "ADMIN".Equals((string)rdr["Role"]) ? Role.ADMIN : Role.USER
                , (bool)rdr["isActive"]
                , (DateTime)rdr["Created"]
                , (DateTime)rdr["LastUpdated"]);
            return result;
        }
        #endregion
    }
}
