using MmgMapAPI.Entities;
using MmgMapAPI.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Type = MmgMapAPI.Entities.Type;

namespace MmgMapAPI.DAO
{
    public class YearPlanDAO
    {
        private readonly string CONNECTION_STRING = ConnectionManager.GetConnectionString();

        #region Основные методы
        //Возвращает годовые планы по МО и году
        public List<YearPlan> FindByYearAndMo(int year, int mosId)
        {
            List<YearPlan> result = new List<YearPlan>();
            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"SELECT yp.\"Id\" as \"ypId\", yp.\"Year\" as \"ypYear\", yp.\"Plan\" as \"ypPlan\", yp.\"Created\" as \"ypCreated\", yp.\"LastUpdated\" as \"ypLastUpdated\", t.\"Id\" as \"tId\", t.\"Value\" as \"tValue\" " +
                    $"FROM \"YearPlans\" yp LEFT join \"Types\" t on (yp.\"TypeId\" = t.\"Id\")  WHERE \"Year\" = @year AND \"MosId\" = @mosId";

                //string commandText = $"SELECT * FROM \"YearPlans\" WHERE \"Year\" = @year AND \"MosId\" = @mosId";

                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("year", year);
                    cmd.Parameters.AddWithValue("mosId", mosId);

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                YearPlan yp = new YearPlan((int)rdr["ypId"]
                                                    , (int)rdr["ypYear"]
                                                    , (int)rdr["ypPlan"]
                                                    , (DateTime)rdr["ypCreated"]
                                                    , (DateTime)rdr["ypLastUpdated"]);

                                Type type = new Type((int)rdr["tId"], (string)rdr["tValue"]);
                                yp.Type = type;

                                result.Add(yp);
                                //result.Add(GetYearPlanFromReader(rdr));
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

        //Сохраняет YearPlan в БД и возвращает его с заполненными полями Id, Created и LastUpdated.
        public YearPlan Create(YearPlan yearPlan, int mosId)
        {
            YearPlan result = yearPlan;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"INSERT INTO \"YearPlans\" (\"Year\", \"MosId\", \"TypeId\", \"Plan\", \"Created\", \"LastUpdated\") VALUES (@year, @mosId, @typeId, @plan, current_timestamp, current_timestamp)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("year", yearPlan.Year);
                    cmd.Parameters.AddWithValue("mosId", mosId);
                    cmd.Parameters.AddWithValue("typeId", yearPlan.Type.Id);
                    cmd.Parameters.AddWithValue("plan", yearPlan.Plan);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = FindByYearMosIdAndType(conn, yearPlan.Year, mosId, yearPlan.Type.Id);
                        result.Type = yearPlan.Type.Id == 1 ? Type.GetType1() : Type.GetType2();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return result;
        }

        //Обновляет в БД поля Username, Role и isActive переданного в качестве аргумента User.
        public YearPlan Update(YearPlan yearPlan, int mosId)
        {
            YearPlan result = yearPlan;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"UPDATE \"YearPlans\" SET \"Year\" = @year, \"MosId\" = @mosId, \"TypeId\" = @typeId, \"Plan\" = @plan, \"LastUpdated\" = current_timestamp WHERE \"Id\" = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("year", yearPlan.Year);
                    cmd.Parameters.AddWithValue("mosId", mosId);
                    cmd.Parameters.AddWithValue("typeId", yearPlan.Type.Id);
                    cmd.Parameters.AddWithValue("plan", yearPlan.Plan);
                    cmd.Parameters.AddWithValue("id", yearPlan.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = FindByYearMosIdAndType(conn, yearPlan.Year, mosId, yearPlan.Type.Id);
                        result.Type = yearPlan.Type.Id == 1 ? Type.GetType1() : Type.GetType2();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            return result;
        }

        //Обновляет в БД поля Username, Role и isActive переданного в качестве аргумента User.
        public bool Delete(int yearPlanId)
        {
            bool result = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"DELETE FROM \"YearPlans\" WHERE \"Id\" = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("id", yearPlanId);

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
        private YearPlan FindByYearMosIdAndType(NpgsqlConnection conn, int year, int mosId, int typeId)
        {
            YearPlan result = null;

            string commandText = $"SELECT * FROM \"YearPlans\" WHERE \"Year\"= @year AND \"MosId\"= @mosId AND \"TypeId\" = @typeId";
            using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.Parameters.AddWithValue("year", year);
                cmd.Parameters.AddWithValue("mosId", mosId);
                cmd.Parameters.AddWithValue("typeId", typeId);

                //Я не использую try catch для следующего обращения к БД т.к. метод вспомогательный, а исключения обработаны в вызывающих методах
                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    result = GetYearPlanFromReader(rdr);
                }
            }
            return result;
        }

        //Возвращает объект Week из объекта NpgsqlDataReader
        private YearPlan GetYearPlanFromReader(NpgsqlDataReader rdr)
        {
            YearPlan result = new YearPlan((int)rdr["Id"]
                , (int)rdr["Year"]
                , (int)rdr["Plan"]
                , (DateTime)rdr["Created"]
                , (DateTime)rdr["LastUpdated"]);
            return result;
        }
        #endregion
    }
}
