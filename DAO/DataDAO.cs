using MmgMapAPI.Entities;
using MmgMapAPI.Utils;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MmgMapAPI.DAO
{
    public class DataDAO
    {
        private readonly string CONNECTION_STRING = ConnectionManager.GetConnectionString();
        private readonly string GET_FULL_DATA_COMMAND_STRING = $"select d.\"Id\" as \"DataId\", d.\"IsMammographAvaliable\", d.\"IsMammographWasWorking\", d.\"NumberOfPerformedMammography\", d.\"NumberOfDetectedPathology\", d.\"Comment\", d.\"Created\", d.\"LastUpdated\"" +
                    $", m1.\"Id\" as \"m1Id\", m1.\"Name\" as \"m1Name\", m1.\"Address\" as \"m1Address\", m1.\"Coordinates\" as \"m1Coordinates\", m1.\"ResponsibleEmployeeFIO\" as \"m1ResponsibleEmployeeFIO\", m1.\"ResponsibleEmployeePhoneNumber\" as \"m1ResponsibleEmployeePhoneNumber\", m1.\"ResponsibleEmployeeEmail\" as \"m1ResponsibleEmployeeEmail\", m1.\"Created\" as \"m1Created\", m1.\"LastUpdated\" as \"m1LastUpdated\"" +
                    $", m2.\"Id\" as \"m2Id\", m2.\"Name\" as \"m2Name\", m2.\"Address\" as \"m2Address\", m2.\"Coordinates\" as \"m2Coordinates\", m2.\"ResponsibleEmployeeFIO\" as \"m2ResponsibleEmployeeFIO\", m2.\"ResponsibleEmployeePhoneNumber\" as \"m2ResponsibleEmployeePhoneNumber\", m2.\"ResponsibleEmployeeEmail\" as \"m2ResponsibleEmployeeEmail\", m2.\"Created\" as \"m2Created\", m2.\"LastUpdated\" as \"m2LastUpdated\"" +
                    $", yp.\"Id\" as \"ypId\", yp.\"Year\" as \"ypYear\", yp.\"Plan\" as \"ypPlan\", yp.\"Created\" as \"ypCreated\", yp.\"LastUpdated\" as \"ypLastUpdated\"" +
                    $", t.\"Id\" as \"tId\", t.\"Value\" as \"tValue\" " +
                    $", w.\"Id\" as \"wId\", w.\"Year\" as \"wYear\", w.\"NumberInYear\" as \"wNumberInYear\", w.\"Target\" as \"wTarget\", w.\"FirstDay\" as \"wFirstDay\", w.\"EndDay\" as \"wEndDay\", w.\"Created\" as \"wCreated\", w.\"LastUpdated\" as \"wLastUpdated\" " +
                    $"from \"Data\" d " +
                    $"left join \"Mos\" m1 on(\"MoWithAttachedPopulation\" = m1.\"Id\") " +
                    $"left join \"Mos\" m2 on(\"MoPerformedMammography\" = m2.\"Id\") " +
                    $"left join \"YearPlans\" yp ON(d.\"YearPlan\" = yp.\"Id\") " +
                    $"left join \"Types\" t ON(d.\"Type\" = t.\"Id\") " +
                    $"left join \"Weeks\" w ON(d.\"Week\" = w.\"Id\") " +
                    $"WHERE d.\"MoWithAttachedPopulation\" = @mosId AND d.\"Week\" = @weekId";

        #region Основные методы
        //Возвращает все объекты FullData из БД относящиеся к указанным МО и отчётной неделе
        public List<FullData> FindFullDataByMoAndWeek(int mosId, int weekId)
        {
            List<FullData> result = new List<FullData>();

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(GET_FULL_DATA_COMMAND_STRING, conn))
                {
                    cmd.Parameters.AddWithValue("mosId", mosId);
                    cmd.Parameters.AddWithValue("weekId", weekId);

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                result.Add(GetFullDataFromReader(rdr));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                result.ForEach(fd =>
                {
                    Dictionary<string, int> summ = AddDataByYear(conn, fd.YearPlan.Year, fd.MoWithAttachedPopulation.Id);
                    if (fd.Type.Id == 1)
                    {
                        fd.NumberOfPerformedMammographyByYear = summ["PerformedType1"];
                        fd.NumberOfDetectedPathologyByYear = summ["DetectedType1"];
                    }
                    else
                    {
                        fd.NumberOfPerformedMammographyByYear = summ["PerformedType2"];
                        fd.NumberOfDetectedPathologyByYear = summ["DetectedType2"];

                        List<Mo> MosForWhichMammographyWasPerformed = GetMosForWhichMammographyWasPerformed(conn, (int)fd.Id);
                        fd.MosForWhichMammographyWasPerformed = MosForWhichMammographyWasPerformed;
                    }
                });
            }
            return result;
        }

        public PartialData FindPartialData(int mosId, int weekId)
        {
            PartialData result = null;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandString = $"select d.\"Id\" as \"dId\", yp.\"Plan\" as \"ypPlan\", w.\"Year\" as \"wYear\", w.\"NumberInYear\" as \"wNumberInYear\" " +
                    $"from \"Data\" d left join \"YearPlans\" yp on (\"YearPlan\" = yp.\"Id\") left join \"Weeks\" w on (\"Week\" = w.\"Id\") " +
                    $"where \"MoWithAttachedPopulation\" = @mosId and \"Week\" = @weekId and \"Type\" = 1";
                using (NpgsqlCommand cmd = new NpgsqlCommand(commandString, conn))
                {
                    cmd.Parameters.AddWithValue("mosId", mosId);
                    cmd.Parameters.AddWithValue("weekId", weekId);

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            result = new PartialData(
                                (int)rdr["dId"]
                                , (int)rdr["ypPlan"]
                                , (int)rdr["wYear"]
                                , (int)rdr["wNumberInYear"]);
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                result.PercentageOfCompletion = ((decimal)AddDataByYear(conn, result.Year, mosId)["PerformedType1"] / (decimal)result.YearPlan) * 100;
            }
            return result;
        }

        //Создаёт новый объект Data
        public FullData Create(FullData fullData, int mosId, int weekId)
        {
            FullData result = fullData;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"INSERT INTO \"Data\" (\"MoWithAttachedPopulation\"" +
                    (fullData.MoPerformedMammography != null ? $", \"MoPerformedMammography\"" : "") +
                    $", \"IsMammographAvaliable\", \"IsMammographWasWorking\", \"YearPlan\", \"Week\", \"Type\", \"NumberOfPerformedMammography\", \"NumberOfDetectedPathology\", \"Comment\", \"Created\", \"LastUpdated\") " +
                    $"VALUES (@moWithAttachedPopulation" +
                    (fullData.MoPerformedMammography != null ? $", @moPerformedMammography" : "") +
                    $", @isMammographAvaliable, @isMammographWasWorking, @yearPlan, @week, @type, @numberOfPerformedMammography, @numberOfDetectedPathology, @comment, current_timestamp, current_timestamp)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("moWithAttachedPopulation", mosId);

                    if (fullData.MoPerformedMammography != null)
                    {
                        cmd.Parameters.AddWithValue("moPerformedMammography", fullData.MoPerformedMammography.Id);
                    }

                    cmd.Parameters.AddWithValue("isMammographAvaliable", fullData.IsMammographAvaliable);
                    cmd.Parameters.AddWithValue("isMammographWasWorking", fullData.IsMammographWasWorking);
                    cmd.Parameters.AddWithValue("yearPlan", fullData.YearPlan.Id);
                    cmd.Parameters.AddWithValue("week", weekId);
                    cmd.Parameters.AddWithValue("type", fullData.Type.Id);
                    cmd.Parameters.AddWithValue("numberOfPerformedMammography", fullData.NumberOfPerformedMammography);
                    cmd.Parameters.AddWithValue("numberOfDetectedPathology", fullData.NumberOfDetectedPathology);
                    cmd.Parameters.AddWithValue("comment", fullData.Comment);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        var asd = ex;
                        return null;
                    }
                }
                using (NpgsqlCommand cmd = new NpgsqlCommand(GET_FULL_DATA_COMMAND_STRING + " AND d.\"Type\" = @typeId", conn))
                {
                    cmd.Parameters.AddWithValue("mosId", mosId);
                    cmd.Parameters.AddWithValue("weekId", weekId);
                    cmd.Parameters.AddWithValue("typeId", fullData.Type.Id);

                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
                        result = GetFullDataFromReader(rdr);
                    }
                }
            }
            return result;
        }

        //Изменяет объект Data
        public FullData Update(FullData fullData)
        {
            FullData result = fullData;

            using (NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string commandText = $"UPDATE \"Data\" SET \"MoWithAttachedPopulation\" = @moWithAttachedPopulation, " +
                    (fullData.MoPerformedMammography != null ? $"\"MoPerformedMammography\" = @moPerformedMammography, " : "") +
                    //(fullData.MoForWhichMammographyWasPerformed != null ? $"\"MoForWhichMammographyWasPerformed\" = @moForWhichMammographyWasPerformed, " : "") +
                    $"\"IsMammographAvaliable\" = @isMammographAvaliable, \"IsMammographWasWorking\" = @isMammographWasWorking,\"YearPlan\" = @yearPlan, \"Week\" = @week, \"Type\" = @type, \"NumberOfPerformedMammography\" = @numberOfPerformedMammography, \"NumberOfDetectedPathology\" = @numberOfDetectedPathology, \"Comment\" = @comment, \"LastUpdated\" = current_timestamp WHERE \"Id\" = @id";

                using (NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("moWithAttachedPopulation", fullData.MoWithAttachedPopulation.Id);
                    if (fullData.MoPerformedMammography != null)
                    {
                        cmd.Parameters.AddWithValue("moPerformedMammography", fullData.MoPerformedMammography.Id);
                    }
                    //if (fullData.MoForWhichMammographyWasPerformed != null)
                    //{
                    //    cmd.Parameters.AddWithValue("moForWhichMammographyWasPerformed", fullData.MoForWhichMammographyWasPerformed.Id);
                    //}
                    cmd.Parameters.AddWithValue("isMammographAvaliable", fullData.IsMammographAvaliable);
                    cmd.Parameters.AddWithValue("isMammographWasWorking", fullData.IsMammographWasWorking);
                    cmd.Parameters.AddWithValue("yearPlan", fullData.YearPlan.Id);
                    cmd.Parameters.AddWithValue("week", fullData.Week.Id);
                    cmd.Parameters.AddWithValue("type", fullData.Type.Id);
                    cmd.Parameters.AddWithValue("numberOfPerformedMammography", fullData.NumberOfPerformedMammography);
                    cmd.Parameters.AddWithValue("numberOfDetectedPathology", fullData.NumberOfDetectedPathology);
                    cmd.Parameters.AddWithValue("comment", fullData.Comment);
                    cmd.Parameters.AddWithValue("id", fullData.Id);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        return null;
                    }
                }

                string GetAfterUpdateCommand = GET_FULL_DATA_COMMAND_STRING.Split("WHERE")[0] + "WHERE d.\"Id\" = @dataId";

                using (NpgsqlCommand cmd = new NpgsqlCommand(GetAfterUpdateCommand, conn))
                {
                    cmd.Parameters.AddWithValue("dataId", fullData.Id);

                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
                        result = GetFullDataFromReader(rdr);
                    }
                }
            }
            return result;
        }
        #endregion

        #region Вспомогательные методы
        //Возвращает объект Data из объекта NpgsqlDataReader
        private FullData GetFullDataFromReader(NpgsqlDataReader rdr)
        {
            FullData fullData = new FullData();

            fullData.Id = (int)rdr["DataId"];
            fullData.IsMammographAvaliable = (bool)rdr["IsMammographAvaliable"];
            fullData.IsMammographWasWorking = (bool)rdr["IsMammographWasWorking"];
            fullData.NumberOfPerformedMammography = (int)rdr["NumberOfPerformedMammography"];
            fullData.NumberOfDetectedPathology = (int)rdr["NumberOfDetectedPathology"];
            fullData.Comment = (string)rdr["Comment"];
            fullData.Created = (DateTime)rdr["Created"];
            fullData.Created = (DateTime)rdr["LastUpdated"];

            Mo moWithAttachedPopulation = new Mo((int)rdr["m1Id"]
               , (string)rdr["m1Name"]
               , (string)rdr["m1Address"]
               , (string)rdr["m1Coordinates"]
               , (string)rdr["m1ResponsibleEmployeeFIO"]
               , (string)rdr["m1ResponsibleEmployeePhoneNumber"]
               , (string)rdr["m1ResponsibleEmployeeEmail"]
               , (DateTime)rdr["m1Created"]
               , (DateTime)rdr["m1LastUpdated"]);

            Mo moPerformedMammography;
            try
            {
                moPerformedMammography = new Mo((int)rdr["m2Id"]
                    , (string)rdr["m2Name"]
                    , (string)rdr["m2Address"]
                    , (string)rdr["m2Coordinates"]
                    , (string)rdr["m2ResponsibleEmployeeFIO"]
                    , (string)rdr["m2ResponsibleEmployeePhoneNumber"]
                    , (string)rdr["m2ResponsibleEmployeeEmail"]
                    , (DateTime)rdr["m2Created"]
                    , (DateTime)rdr["m2LastUpdated"]);
            }
            catch
            {
                moPerformedMammography = null;
            }

            //Mo moForWhichMammographyWasPerformed;
            //try
            //{
            //    moForWhichMammographyWasPerformed = new Mo((int)rdr["m3Id"]
            //        , (string)rdr["m3Name"]
            //        , (string)rdr["m3Address"]
            //        , (string)rdr["m3Coordinates"]
            //        , (string)rdr["m3ResponsibleEmployeeFIO"]
            //        , (string)rdr["m3ResponsibleEmployeePhoneNumber"]
            //        , (string)rdr["m3ResponsibleEmployeeEmail"]
            //        , (DateTime)rdr["m3Created"]
            //        , (DateTime)rdr["m3LastUpdated"]);
            //}
            //catch
            //{
            //    moForWhichMammographyWasPerformed = null;
            //}

            fullData.MoWithAttachedPopulation = moWithAttachedPopulation;
            fullData.MoPerformedMammography = moPerformedMammography;
            //fullData.MoForWhichMammographyWasPerformed = moForWhichMammographyWasPerformed;

            Entities.Type type = new Entities.Type((int)rdr["tId"], (string)rdr["tValue"]);

            YearPlan yearPlan = new YearPlan();
            yearPlan.Id = (int)rdr["ypId"];
            yearPlan.Year = (int)rdr["ypYear"];
            yearPlan.Type = type;
            yearPlan.Plan = (int)rdr["ypPlan"];
            yearPlan.Created = (DateTime)rdr["ypCreated"];
            yearPlan.LastUpdated = (DateTime)rdr["ypLastUpdated"];

            Week week = new Week((int)rdr["wId"]
                , (int)rdr["wYear"]
                , (int)rdr["wNumberInYear"]
                , (decimal)rdr["wTarget"]
                , (DateTime)rdr["wFirstDay"]
                , (DateTime)rdr["wEndDay"]
                , (DateTime)rdr["wCreated"]
                , (DateTime)rdr["wLastUpdated"]);

            fullData.YearPlan = yearPlan;
            fullData.Type = type;
            fullData.Week = week;

            return fullData;
        }

        //Считает и добавляет соответстующим полям объекта Data результаты по году (нарастающий итог)
        private Dictionary<string, int> AddDataByYear(NpgsqlConnection conn, int year, int mosId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            List<int> weeksId = new List<int>();
            string weeksIdCommandString = $"SELECT * FROM \"Weeks\" WHERE \"Year\" = @year";
            using (NpgsqlCommand cmd = new NpgsqlCommand(weeksIdCommandString, conn))
            {
                cmd.Parameters.AddWithValue("year", year);

                try
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            weeksId.Add((int)rdr["Id"]);
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            if (weeksId.Count > 0)
            {
                string getSummCommandString = $"SELECT SUM(\"NumberOfPerformedMammography\") AS \"Performed\", SUM(\"NumberOfDetectedPathology\") AS \"Detected\" FROM \"Data\" WHERE \"MoWithAttachedPopulation\" = @mosId AND \"Type\" = @type AND (";

                weeksId.ForEach(id =>
                {
                    getSummCommandString += " \"Week\" = @weekId" + id;
                    if (weeksId.IndexOf(id) != weeksId.Count - 1)
                    {
                        getSummCommandString += " OR";
                    }
                    else
                    {
                        getSummCommandString += ")";
                    }
                });

                //Получаю суммы с типом данных 1 (с прикреплённым населением)
                using (NpgsqlCommand cmd = new NpgsqlCommand(getSummCommandString, conn))
                {
                    cmd.Parameters.AddWithValue("mosId", mosId);

                    weeksId.ForEach(id =>
                    {
                        cmd.Parameters.AddWithValue("weekId" + id, id);
                    });

                    cmd.Parameters.AddWithValue("type", 1);

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            result.Add("PerformedType1", (int)(long)rdr["Performed"]);
                            result.Add("DetectedType1", (int)(long)rdr["Detected"]);
                        }
                    }
                    catch (Exception)
                    {
                        result.Add("PerformedType1", 0);
                        result.Add("DetectedType1", 0);
                    }
                }

                //Получаю суммы с типом данных 2 (по самостоятельному тарифу)
                using (NpgsqlCommand cmd = new NpgsqlCommand(getSummCommandString, conn))
                {
                    cmd.Parameters.AddWithValue("mosId", mosId);

                    weeksId.ForEach(id =>
                    {
                        cmd.Parameters.AddWithValue("weekId" + id, id);
                    });

                    cmd.Parameters.AddWithValue("type", 2);

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            rdr.Read();
                            result.Add("PerformedType2", (int)(long)rdr["Performed"]);
                            result.Add("DetectedType2", (int)(long)rdr["Detected"]);
                        }
                    }
                    catch (Exception)
                    {
                        result.Add("PerformedType2", 0);
                        result.Add("DetectedType2", 0);
                    }
                }
            }
            return result;
        }

        //Возвращает список МО относящихся к данному объекту Data (только для Data у которых Type.Id = 2 "Работающие по самостоятельному тарифу")
        private List<Mo> GetMosForWhichMammographyWasPerformed(NpgsqlConnection conn, int dataId)
        {
            List<Mo> result = new List<Mo>();

            string getMosIdCommandString = $"SELECT * FROM \"DataMos\" WHERE \"DataId\" = @dataId";
            List<int> mosId = new List<int>();
            using (NpgsqlCommand cmd = new NpgsqlCommand(getMosIdCommandString, conn))
            {
                cmd.Parameters.AddWithValue("dataId", dataId);

                try
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            mosId.Add((int)rdr["MosId"]);
                        }
                    }
                }
                catch (Exception)
                {
                    return result;
                }
            }

            if (mosId.Count > 0)
            {
                string getMosCommandString = $"SELECT * FROM \"Mos\" WHERE";
                mosId.ForEach(id =>
                {
                    getMosCommandString += " \"Id\" = @id" + id;
                    if (mosId.IndexOf(id) != mosId.Count - 1)
                    {
                        getMosCommandString += " OR";
                    }
                });

                using (NpgsqlCommand cmd = new NpgsqlCommand(getMosCommandString, conn))
                {
                    mosId.ForEach(id =>
                    {
                        cmd.Parameters.AddWithValue("id" + id, id);
                    });

                    try
                    {
                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                result.Add(new Mo((int)rdr["Id"]
                                    , (string)rdr["Name"]
                                    , (string)rdr["Address"]
                                    , (string)rdr["Coordinates"]
                                    , (string)rdr["ResponsibleEmployeeFIO"]
                                    , (string)rdr["ResponsibleEmployeePhoneNumber"]
                                    , (string)rdr["ResponsibleEmployeeEmail"]
                                    , (DateTime)rdr["Created"]
                                    , (DateTime)rdr["LastUpdated"]));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return result;
                    }
                }
            }
            return result;
        }
        #endregion
    }
}
