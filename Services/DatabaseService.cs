using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using CourseCenterWPF.Models;

namespace CourseCenterWPF.Services
{
    public class DatabaseService
    {
        // ⚠️ ИЗМЕНИ СТРОКУ ПОДКЛЮЧЕНИЯ!
        private string connectionString = "Server=localhost;Database=coursecenter;Uid=root;Pwd=;Port=3306;";

        public DataTable ExecuteQuery(string query)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database error: {ex.Message}");
            }
        }

        public int ExecuteNonQuery(string query)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Query error: {ex.Message}");
            }
        }

        public List<Cursant> GetCursanti()
        {
            var list = new List<Cursant>();
            DataTable dt = ExecuteQuery("SELECT * FROM Cursant ORDER BY Nume");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Cursant
                {
                    IdCursant = Convert.ToInt32(row["IdCursant"]),
                    Nume = row["Nume"].ToString(),
                    Prenume = row["Prenume"].ToString(),
                    Telefon = row["Telefon"].ToString(),
                    Email = row["Email"].ToString()
                });
            }
            return list;
        }

        public List<Curs> GetCursuri()
        {
            var list = new List<Curs>();
            DataTable dt = ExecuteQuery("SELECT * FROM Curs ORDER BY Denumire");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Curs
                {
                    IdCurs = Convert.ToInt32(row["IdCurs"]),
                    Denumire = row["Denumire"].ToString(),
                    Formator = row["Formator"].ToString(),
                    Pret = Convert.ToDecimal(row["Pret"]),
                    DurataZile = Convert.ToInt32(row["DurataZile"])
                });
            }
            return list;
        }
    }
}