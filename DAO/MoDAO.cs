using MmgMapAPI.Entities;
using MmgMapAPI.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.DAO
{
    public class MoDAO
    {
        private readonly string CONNECTION_STRING = ConnectionManager.GetConnectionString();

        #region Основные методы
        //Возвращает все МО из БД
        public List<Mo> FindAll()
        {
            List<Mo> result = new List<Mo>();
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"SELECT * FROM \"Mos\"";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                result.Add(GetMoFromReader(rdr));
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

        //Возвращает MO по Id или null если такой MO нет.
        public Mo FindById(int Id)
        {
            Mo result = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                try
                {
                    result = FindMoById(conn, Id);
                }
                catch
                {
                    return null;
                }
            }
            return result;
        }

        //Возвращает MO по Name или null если такой MO нет.
        public Mo FindByName(string name)
        {
            Mo result = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                try
                {
                    result = FindByName(conn, name);
                }
                catch
                {
                    return null;
                }
            }
            return result;
        }

        //Возвращает принадлежащие пользователю МО по его Id
        public List<Mo> FindByUserId(int userId)
        {
            List<Mo> result = new List<Mo>();
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                List<int> mosIdList = new List<int>();

                string userMosCommand = $"SELECT * FROM \"UsersMos\" WHERE \"UserId\" = @userId";
                using (NpgsqlCommand cmd = new NpgsqlCommand(userMosCommand, conn))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                mosIdList.Add((int)rdr["MosId"]);
                            }
                        }
                    }
                    catch
                    {
                        //Возвращаю пустой список МО в случае ошибки чтения
                        return result;
                    }
                }

                if (mosIdList.Count > 0)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(GetCommandString(mosIdList), conn))
                    {
                        mosIdList.ForEach(id =>
                        {
                            cmd.Parameters.AddWithValue("value" + id, id);
                        });

                        try
                        {
                            using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    result.Add(GetMoFromReader(rdr));
                                }
                            }
                        }
                        catch
                        {
                            //Возвращаю пустой список МО в случае ошибки чтения
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        //Возвращает список МО принадлежащих объекту FullData в качестве организаций для которых выполняется маммография (при работе по самостоятельному тарифу)
        public List<Mo> FindByDataId(int dataId)
        {
            List<Mo> result = new List<Mo>();
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                List<int> mosIdList = new List<int>();

                string userMosCommand = $"SELECT * FROM \"DataMos\" WHERE \"DataId\" = @dataId";
                using (NpgsqlCommand cmd = new NpgsqlCommand(userMosCommand, conn))
                {
                    cmd.Parameters.AddWithValue("dataId", dataId);
                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                mosIdList.Add((int)rdr["MosId"]);
                            }
                        }
                    }
                    catch
                    {
                        //Возвращаю пустой список МО в случае ошибки чтения
                        return result;
                    }
                }

                if (mosIdList.Count > 0)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(GetCommandString(mosIdList), conn))
                    {
                        mosIdList.ForEach(id =>
                        {
                            cmd.Parameters.AddWithValue("value" + id, id);
                        });

                        try
                        {
                            using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    result.Add(GetMoFromReader(rdr));
                                }
                            }
                        }
                        catch
                        {
                            //Возвращаю пустой список МО в случае ошибки чтения
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        //Сохраняет MO в БД и возвращает его с заполненными полями Id, Created и LastUpdated.
        public Mo Create(Mo mo)
        {
            Mo result = mo;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"INSERT INTO \"Mos\" (\"Name\", \"Address\", \"Coordinates\", \"ResponsibleEmployeeFIO\", \"ResponsibleEmployeePhoneNumber\", \"ResponsibleEmployeeEmail\", \"Created\", \"LastUpdated\") VALUES (@name, @address, @coordinates, @fio, @phoneNumber, @email, current_timestamp, current_timestamp)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("name", mo.Name);
                    cmd.Parameters.AddWithValue("address", mo.Address);
                    cmd.Parameters.AddWithValue("coordinates", mo.Coordinates);
                    cmd.Parameters.AddWithValue("fio", mo.ResponsibleEmployeeFIO != null ? mo.ResponsibleEmployeeFIO : "");
                    cmd.Parameters.AddWithValue("phoneNumber", mo.ResponsibleEmployeePhoneNumber != null ? mo.ResponsibleEmployeePhoneNumber : "");
                    cmd.Parameters.AddWithValue("email", mo.ResponsibleEmployeeEmail != null ? mo.ResponsibleEmployeeEmail : "");

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        var asd = e;
                        return null;
                    }
                }

                result = FindByName(conn, mo.Name);

            }
            return result;
        }

        //Обновляет в БД объект Mo.
        public Mo Update(Mo mo)
        {
            Mo result = mo;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"UPDATE \"Mos\" SET \"Name\" = @name, \"Address\" = @address, \"Coordinates\" = @coordinates, \"ResponsibleEmployeeFIO\" = @fio, \"ResponsibleEmployeePhoneNumber\" = @phoneNumber, \"ResponsibleEmployeeEmail\" = @email, \"LastUpdated\" = current_timestamp WHERE \"Id\" = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("name", mo.Name);
                    cmd.Parameters.AddWithValue("address", mo.Address);
                    cmd.Parameters.AddWithValue("coordinates", mo.Coordinates);
                    cmd.Parameters.AddWithValue("fio", mo.ResponsibleEmployeeFIO);
                    cmd.Parameters.AddWithValue("phoneNumber", mo.ResponsibleEmployeePhoneNumber);
                    cmd.Parameters.AddWithValue("email", mo.ResponsibleEmployeeEmail);
                    cmd.Parameters.AddWithValue("id", mo.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = FindMoById(conn, mo.Id);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return result;
        }

        //Делает медицинские организации из списка доступными для пользователя
        public void AddMosToUser(List<Mo> mos, int userId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"INSERT INTO \"UsersMos\" (\"UserId\", \"MosId\", \"Created\") VALUES";
                mos.ForEach(mo =>
                {
                    commandText += " (@userId" + mo.Id + ", @moId" + mo.Id + ", current_timestamp)";
                    if (mos.IndexOf(mo) != mos.Count - 1)
                    {
                        commandText += ", ";
                    }
                });

                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {

                    mos.ForEach(mo =>
                    {
                        cmd.Parameters.AddWithValue("userId" + mo.Id, userId);
                        cmd.Parameters.AddWithValue("moId" + mo.Id, mo.Id);
                    });

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        Console.WriteLine("Здесь должна быть логика обработки ошибки, но я похоже не успеваю её написать. Я бы создал таблицу логов в БД и писал туда обо всех таких происшествиях.");
                    }
                }
            }
        }

        //Делает медицинские организации из списка не доступными для пользователя
        public void RemoveMosFromUser(List<Mo> mos, int userId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"DELETE FROM \"UsersMos\" WHERE";
                mos.ForEach(mo =>
                {
                    commandText += " (\"UserId\" = @userId" + mo.Id + " AND" + " \"MosId\" = @moId" + mo.Id + ")" ;
                    if (mos.IndexOf(mo) != mos.Count - 1)
                    {
                        commandText += " OR";
                    }
                });

                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {

                    mos.ForEach(mo =>
                    {
                        cmd.Parameters.AddWithValue("userId" + mo.Id, userId);
                        cmd.Parameters.AddWithValue("moId" + mo.Id, mo.Id);
                    });

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        Console.WriteLine("Здесь должна быть логика обработки ошибки, но я похоже не успеваю её написать. Я бы создал таблицу логов в БД и писал туда обо всех таких происшествиях.");
                    }
                }
            }
        }

        //Добавляет медицинские организации в список МО для которых выполняется маммография (при работе по самостоятельному тарифу)
        public void AddMosToData(List<Mo> mos, int dataId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"INSERT INTO \"DataMos\" (\"DataId\", \"MosId\", \"Created\") VALUES";
                mos.ForEach(mo =>
                {
                    commandText += " (@dataId" + mo.Id + ", @moId" + mo.Id + ", current_timestamp)";
                    if (mos.IndexOf(mo) != mos.Count - 1)
                    {
                        commandText += ", ";
                    }
                });

                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {

                    mos.ForEach(mo =>
                    {
                        cmd.Parameters.AddWithValue("dataId" + mo.Id, dataId);
                        cmd.Parameters.AddWithValue("moId" + mo.Id, mo.Id);
                    });

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception)
                    { 
                        Console.WriteLine("Здесь должна быть логика обработки ошибки, но я похоже не успеваю её написать. Я бы создал таблицу логов в БД и писал туда обо всех таких происшествиях.");
                    }
                }
            }
        }

        //Удаляет медицинские организации из списка МО для которых выполняется маммография (при работе по самостоятельному тарифу)
        public void RemoveMosFromData(List<Mo> mos, int dataId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"DELETE FROM \"DataMos\" WHERE";
                mos.ForEach(mo =>
                {
                    commandText += " (\"DataId\" = @dataId" + mo.Id + " AND" + " \"MosId\" = @moId" + mo.Id + ")";
                    if (mos.IndexOf(mo) != mos.Count - 1)
                    {
                        commandText += " OR";
                    }
                });

                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {

                    mos.ForEach(mo =>
                    {
                        cmd.Parameters.AddWithValue("dataId" + mo.Id, dataId);
                        cmd.Parameters.AddWithValue("moId" + mo.Id, mo.Id);
                    });

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        Console.WriteLine("Здесь должна быть логика обработки ошибки, но я похоже не успеваю её написать. Я бы создал таблицу логов в БД и писал туда обо всех таких происшествиях.");
                    }
                }
            }
        }
        #endregion

        #region Вспомогательные методы
        //Возвращает МО по Name или null если такого МО нет.
        //Принимает NpgsqlConnection в качестве аргумента (что бы не открывать лишние соединения) и используется в методах Update(Мо мо) и FindById(int Id).
        public Mo FindMoById(NpgsqlConnection conn, int id)
        {
            Mo result = null;

            string commandText = $"SELECT * FROM \"Mos\" WHERE \"Id\"= @id";
            using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.Parameters.AddWithValue("id", id);

                //Я не использую try catch для следующего обращения к БД т.к. метод вспомогательный, а исключения обработаны в вызывающих методах
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    result = GetMoFromReader(rdr);
                }
            }
            return result;
        }

        //Возвращает MO по Name или null если такого MO нет.
        //Принимает NpgsqlConnection в качестве аргумента (что бы не открывать лишние соединения) и используется в методах Create(Mo mo) и FindByUsername(string name).
        private Mo FindByName(NpgsqlConnection conn, string name)
        {
            Mo result = null;

            string commandText = $"SELECT * FROM \"Mos\" WHERE \"Name\"= @name";
            using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.Parameters.AddWithValue("name", name);

                //Я не использую try catch для следующего обращения к БД т.к. метод вспомогательный, а исключения обработаны в вызывающих методах
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    result = GetMoFromReader(rdr);
                }
            }
            return result;
        }

        //Возвращает объект MO из объекта NpgsqlDataReader
        private Mo GetMoFromReader(NpgsqlDataReader rdr)
        {
            Mo result = new Mo((int)rdr["Id"]
                , (string)rdr["Name"]
                , (string)rdr["Address"]
                , (string)rdr["Coordinates"]
                , (string)rdr["ResponsibleEmployeeFIO"]
                , (string)rdr["ResponsibleEmployeePhoneNumber"]
                , (string)rdr["ResponsibleEmployeeEmail"]
                , (DateTime)rdr["Created"]
                , (DateTime)rdr["LastUpdated"]);
            return result;
        }

        //Возвращает строку (SQL запрос) сформированную из списка Id медицинских организаций.
        private string GetCommandString(List<int> mosIdList)
        {
            string result = "SELECT * FROM \"Mos\" WHERE";
            mosIdList.ForEach(id =>
            {
                result += " \"Id\" = @value" + id;
                if (mosIdList.IndexOf(id) != mosIdList.Count - 1)
                {
                    result += " OR";
                }
            });
            return result;
        }
        #endregion
    }
}
