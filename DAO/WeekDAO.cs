using MmgMapAPI.Entities;
using MmgMapAPI.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.DAO
{
    public class WeekDAO
    {
        private readonly string CONNECTION_STRING = ConnectionManager.GetConnectionString();

        #region Основные методы
        //Возвращает все объекты Week из БД относящиеся к указанному году
        public List<Week> FindByYear(int year)
        {
            List<Week> result = new List<Week>();
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"SELECT * FROM \"Weeks\" WHERE \"Year\" = @year";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("year", year);

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                result.Add(GetWeekFromReader(rdr));
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

        //Возвращает все объекты Week из БД относящиеся к указанному году
        public Week FindById(int Id)
        {
            Week result = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"SELECT * FROM \"Weeks\" WHERE \"Id\" = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("id", Id);

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            result = GetWeekFromReader(rdr);
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

        //Возвращает список годов для которых существуют объекты Week
        public List<int> FindYears()
        {
            List<int> result = new List<int>();
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"SELECT DISTINCT \"Year\" FROM \"Weeks\" ORDER BY \"Year\" DESC";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                result.Add((int)rdr["Year"]);
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

        //Сохраняет Week в БД и возвращает его с заполненными полями Id, Created и LastUpdated.
        public Week Create(Week week)
        {
            Week result = week;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"INSERT INTO \"Weeks\" (\"Year\", \"NumberInYear\", \"Target\", \"FirstDay\", \"EndDay\", \"Created\", \"LastUpdated\") VALUES (@year, @number, @target, @firstDay, @endDay, current_timestamp, current_timestamp)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("year", week.Year);
                    cmd.Parameters.AddWithValue("number", week.NumberInYear);
                    cmd.Parameters.AddWithValue("target", week.Target);
                    cmd.Parameters.AddWithValue("firstDay", week.FirstDay);
                    cmd.Parameters.AddWithValue("endDay", week.EndDay);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = FindByYearAndNumber(conn, week.Year, week.NumberInYear);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return result;
        }

        //Обновляет в БД объект Week.
        public Week Update(Week week)
        {
            Week result = week;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"UPDATE \"Weeks\" SET \"Year\" = @year, \"NumberInYear\" = @number, \"Target\" = @target, \"FirstDay\" = @firstDay, \"EndDay\" = @endDay, \"LastUpdated\" = current_timestamp WHERE \"Id\" = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("year", week.Year);
                    cmd.Parameters.AddWithValue("number", week.NumberInYear);
                    cmd.Parameters.AddWithValue("target", week.Target);
                    cmd.Parameters.AddWithValue("firstDay", week.FirstDay);
                    cmd.Parameters.AddWithValue("endDay", week.EndDay);
                    cmd.Parameters.AddWithValue("id", week.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = FindByYearAndNumber(conn, week.Year, week.NumberInYear);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return result;
        }

        //Удаляет отчётную неделю из БД.
        public bool Delete(int weekId)
        {
            bool result = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"DELETE FROM \"Weeks\" WHERE \"Id\" = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("id", weekId);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return result;
        }
        #endregion

        #region Вспомогательные методы
        //Возвращает Week по году и номеру недели в году 
        private Week FindByYearAndNumber(NpgsqlConnection conn, int year, int number)
        {
            Week result = null;

            string commandText = $"SELECT * FROM \"Weeks\" WHERE \"Year\"= @year AND \"NumberInYear\"= @number";
            using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.Parameters.AddWithValue("year", year);
                cmd.Parameters.AddWithValue("number", number);

                //Я не использую try catch для следующего обращения к БД т.к. метод вспомогательный, а исключения обработаны в вызывающих методах
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    result = GetWeekFromReader(rdr);
                }
            }
            return result;
        }

        //Возвращает объект Week из объекта NpgsqlDataReader
        private Week GetWeekFromReader(NpgsqlDataReader rdr)
        {
            Week result = new Week((int)rdr["Id"]
                , (int)rdr["Year"]
                , (int)rdr["NumberInYear"]
                , (decimal)rdr["Target"]
                , (DateTime)rdr["FirstDay"]
                , (DateTime)rdr["EndDay"]
                , (DateTime)rdr["Created"]
                , (DateTime)rdr["LastUpdated"]);
            return result;
        }
        #endregion
    }
}
