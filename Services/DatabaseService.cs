using CourseCenterWPF.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace CourseCenterWPF.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cinema.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        public DataTable ExecuteQuery(string query, params SqliteParameter[] parameters)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(query, conn);
            if (parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }

            var table = new DataTable();
            using var reader = cmd.ExecuteReader();
            table.Load(reader);
            return table;
        }

        public int ExecuteNonQuery(string query, params SqliteParameter[] parameters)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(query, conn);
            if (parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string query, params SqliteParameter[] parameters)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = new SqliteCommand(query, conn);
            if (parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteScalar();
        }

        public List<Film> GetFilme()
        {
            var list = new List<Film>();
            var dt = ExecuteQuery("SELECT IdFilm, Titlu, Gen, DurataMinute, LimitaVarsta FROM Filme ORDER BY Titlu");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Film
                {
                    IdFilm = Convert.ToInt32(row["IdFilm"]),
                    Titlu = row["Titlu"].ToString() ?? string.Empty,
                    Gen = row["Gen"].ToString() ?? string.Empty,
                    DurataMinute = Convert.ToInt32(row["DurataMinute"]),
                    LimitaVarsta = Convert.ToInt32(row["LimitaVarsta"])
                });
            }
            return list;
        }

        private void InitializeDatabase()
        {
            ExecuteNonQuery("PRAGMA foreign_keys = ON;");

            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Filme (
                    IdFilm INTEGER PRIMARY KEY AUTOINCREMENT,
                    Titlu TEXT NOT NULL,
                    Gen TEXT NOT NULL,
                    DurataMinute INTEGER NOT NULL CHECK(DurataMinute > 0),
                    LimitaVarsta INTEGER NOT NULL CHECK(LimitaVarsta >= 0)
                );");

            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Seanse (
                    IdSeansa INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdFilm INTEGER NOT NULL,
                    DataSeansa TEXT NOT NULL,
                    OraSeansa TEXT NOT NULL,
                    PretBilet REAL NOT NULL CHECK(PretBilet > 0),
                    NumarLocuriTotal INTEGER NOT NULL CHECK(NumarLocuriTotal > 0),
                    UNIQUE(IdFilm, DataSeansa, OraSeansa),
                    FOREIGN KEY(IdFilm) REFERENCES Filme(IdFilm) ON DELETE CASCADE
                );");

            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Bilete (
                    IdBilet INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdSeansa INTEGER NOT NULL,
                    ReducereProcent REAL NOT NULL CHECK(ReducereProcent >= 0 AND ReducereProcent <= 100),
                    NumarBilete INTEGER NOT NULL CHECK(NumarBilete > 0),
                    SumaAchitata REAL NOT NULL CHECK(SumaAchitata >= 0),
                    DataVanzare TEXT NOT NULL,
                    FOREIGN KEY(IdSeansa) REFERENCES Seanse(IdSeansa) ON DELETE CASCADE
                );");

            SeedTestDataIfEmpty();
        }

        private void SeedTestDataIfEmpty()
        {
            var filmeCount = Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM Filme") ?? 0);
            if (filmeCount > 0)
            {
                return;
            }

            var filme = new[]
            {
                ("Inception", "Sci-Fi", 148, 12),
                ("The Dark Knight", "Action", 152, 13),
                ("Coco", "Animation", 105, 6),
                ("Interstellar", "Sci-Fi", 169, 12),
                ("La La Land", "Drama", 128, 12)
            };

            foreach (var film in filme)
            {
                ExecuteNonQuery(
                    "INSERT INTO Filme (Titlu, Gen, DurataMinute, LimitaVarsta) VALUES (@titlu, @gen, @durata, @limita)",
                    new SqliteParameter("@titlu", film.Item1),
                    new SqliteParameter("@gen", film.Item2),
                    new SqliteParameter("@durata", film.Item3),
                    new SqliteParameter("@limita", film.Item4));
            }

            var today = DateTime.Today;
            var seanse = new (int filmId, DateTime date, string time, decimal price, int seats)[]
            {
                (1, today, "10:00", 100, 80),
                (2, today, "13:00", 120, 80),
                (3, today, "16:00", 90, 100),
                (4, today, "19:00", 130, 70),
                (5, today, "21:30", 110, 60),
                (1, today.AddDays(1), "11:00", 100, 80),
                (2, today.AddDays(1), "14:00", 120, 80),
                (3, today.AddDays(1), "17:00", 90, 100),
                (4, today.AddDays(2), "18:30", 130, 70),
                (5, today.AddDays(2), "20:30", 110, 60)
            };

            foreach (var seans in seanse)
            {
                ExecuteNonQuery(
                    "INSERT INTO Seanse (IdFilm, DataSeansa, OraSeansa, PretBilet, NumarLocuriTotal) VALUES (@idFilm, @data, @ora, @pret, @locuri)",
                    new SqliteParameter("@idFilm", seans.filmId),
                    new SqliteParameter("@data", seans.date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new SqliteParameter("@ora", seans.time),
                    new SqliteParameter("@pret", seans.price),
                    new SqliteParameter("@locuri", seans.seats));
            }

            for (int i = 1; i <= 20; i++)
            {
                var seansId = (i % 10) + 1;
                var quantity = (i % 4) + 1;
                var discount = (i % 3) * 10m;

                var dt = ExecuteQuery("SELECT PretBilet FROM Seanse WHERE IdSeansa = @id", new SqliteParameter("@id", seansId));
                var price = Convert.ToDecimal(dt.Rows[0]["PretBilet"], CultureInfo.InvariantCulture);
                var paid = Math.Round(price * quantity * (1 - discount / 100m), 2);

                ExecuteNonQuery(
                    "INSERT INTO Bilete (IdSeansa, ReducereProcent, NumarBilete, SumaAchitata, DataVanzare) VALUES (@idSeansa, @reducere, @nr, @suma, @data)",
                    new SqliteParameter("@idSeansa", seansId),
                    new SqliteParameter("@reducere", discount),
                    new SqliteParameter("@nr", quantity),
                    new SqliteParameter("@suma", paid),
                    new SqliteParameter("@data", today.AddDays(-(i % 5)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }
        }
    }
}
