using CourseCenterWPF.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CourseCenterWPF.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pharmacy.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        public DataTable ExecuteQuery(string query, params SqliteParameter[] parameters)
        {
            using var conn = OpenConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            using var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        public int ExecuteNonQuery(string query, params SqliteParameter[] parameters)
        {
            using var conn = OpenConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string query, params SqliteParameter[] parameters)
        {
            using var conn = OpenConnection();
            using var cmd = CreateCommand(conn, query, parameters);
            return cmd.ExecuteScalar();
        }

        public List<Medicament> GetMedicamente(string? searchText = null)
        {
            var query = "SELECT IdMedicament, Denumire, FormaFarmaceutica, Concentratie, Pret, StocCurent FROM Medicamente WHERE 1=1";
            var parameters = new List<SqliteParameter>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query += " AND (Denumire LIKE @q OR FormaFarmaceutica LIKE @q OR Concentratie LIKE @q)";
                parameters.Add(new SqliteParameter("@q", $"%{searchText.Trim()}%"));
            }

            query += " ORDER BY Denumire, FormaFarmaceutica, Concentratie";
            var dt = ExecuteQuery(query, [.. parameters]);
            var list = new List<Medicament>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Medicament
                {
                    IdMedicament = Convert.ToInt32(row["IdMedicament"]),
                    Denumire = row["Denumire"].ToString() ?? string.Empty,
                    FormaFarmaceutica = row["FormaFarmaceutica"].ToString() ?? string.Empty,
                    Concentratie = row["Concentratie"].ToString() ?? string.Empty,
                    Pret = Convert.ToDecimal(row["Pret"], CultureInfo.InvariantCulture),
                    StocCurent = Convert.ToInt32(row["StocCurent"])
                });
            }

            return list;
        }

        public List<Furnizor> GetFurnizori(string? searchText = null)
        {
            var query = "SELECT IdFurnizor, Denumire, Telefon, Email, Adresa FROM Furnizori WHERE 1=1";
            var parameters = new List<SqliteParameter>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query += " AND (Denumire LIKE @q OR Telefon LIKE @q OR Email LIKE @q)";
                parameters.Add(new SqliteParameter("@q", $"%{searchText.Trim()}%"));
            }

            query += " ORDER BY Denumire";
            var dt = ExecuteQuery(query, [.. parameters]);
            var list = new List<Furnizor>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Furnizor
                {
                    IdFurnizor = Convert.ToInt32(row["IdFurnizor"]),
                    Denumire = row["Denumire"].ToString() ?? string.Empty,
                    Telefon = row["Telefon"].ToString() ?? string.Empty,
                    Email = row["Email"].ToString() ?? string.Empty,
                    Adresa = row["Adresa"].ToString() ?? string.Empty
                });
            }

            return list;
        }

        public List<Aprovizionare> GetAprovizionari(int medicamentId = 0, int furnizorId = 0, DateTime? from = null, DateTime? to = null)
        {
            var query = @"
                SELECT a.IdAprovizionare, a.IdMedicament, a.IdFurnizor, a.DataAprovizionare, a.Cantitate, a.PretAchizitie, a.DataExpirare,
                       m.Denumire AS MedicamentDenumire, f.Denumire AS FurnizorDenumire
                FROM Aprovizionari a
                JOIN Medicamente m ON m.IdMedicament = a.IdMedicament
                JOIN Furnizori f ON f.IdFurnizor = a.IdFurnizor
                WHERE 1=1";
            var parameters = new List<SqliteParameter>();

            if (medicamentId > 0)
            {
                query += " AND a.IdMedicament = @medicamentId";
                parameters.Add(new SqliteParameter("@medicamentId", medicamentId));
            }

            if (furnizorId > 0)
            {
                query += " AND a.IdFurnizor = @furnizorId";
                parameters.Add(new SqliteParameter("@furnizorId", furnizorId));
            }

            if (from.HasValue)
            {
                query += " AND a.DataAprovizionare >= @from";
                parameters.Add(new SqliteParameter("@from", from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }

            if (to.HasValue)
            {
                query += " AND a.DataAprovizionare <= @to";
                parameters.Add(new SqliteParameter("@to", to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }

            query += " ORDER BY a.DataAprovizionare DESC, a.IdAprovizionare DESC";

            var dt = ExecuteQuery(query, [.. parameters]);
            var list = new List<Aprovizionare>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Aprovizionare
                {
                    IdAprovizionare = Convert.ToInt32(row["IdAprovizionare"]),
                    IdMedicament = Convert.ToInt32(row["IdMedicament"]),
                    IdFurnizor = Convert.ToInt32(row["IdFurnizor"]),
                    DataAprovizionare = DateTime.Parse(row["DataAprovizionare"].ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    Cantitate = Convert.ToInt32(row["Cantitate"]),
                    PretAchizitie = Convert.ToDecimal(row["PretAchizitie"], CultureInfo.InvariantCulture),
                    DataExpirare = DateTime.Parse(row["DataExpirare"].ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                    MedicamentDenumire = row["MedicamentDenumire"].ToString() ?? string.Empty,
                    FurnizorDenumire = row["FurnizorDenumire"].ToString() ?? string.Empty
                });
            }

            return list;
        }

        public List<RaportItem> GetRaportItems()
        {
            var dt = ExecuteQuery(@"
                SELECT m.Denumire AS MedicamentDenumire,
                       f.Denumire AS FurnizorDenumire,
                       COUNT(*) AS NumarAprovizionari,
                       SUM(a.Cantitate) AS CantitateTotala,
                       SUM(a.Cantitate * a.PretAchizitie) AS CostTotal,
                       m.StocCurent AS StocCurent
                FROM Aprovizionari a
                JOIN Medicamente m ON m.IdMedicament = a.IdMedicament
                JOIN Furnizori f ON f.IdFurnizor = a.IdFurnizor
                GROUP BY m.IdMedicament, f.IdFurnizor, m.Denumire, f.Denumire, m.StocCurent
                ORDER BY m.Denumire, f.Denumire");

            var list = new List<RaportItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new RaportItem
                {
                    MedicamentDenumire = row["MedicamentDenumire"].ToString() ?? string.Empty,
                    FurnizorDenumire = row["FurnizorDenumire"].ToString() ?? string.Empty,
                    NumarAprovizionari = Convert.ToInt32(row["NumarAprovizionari"]),
                    CantitateTotala = Convert.ToInt32(row["CantitateTotala"]),
                    CostTotal = row["CostTotal"] == DBNull.Value ? 0m : Convert.ToDecimal(row["CostTotal"], CultureInfo.InvariantCulture),
                    StocCurent = Convert.ToInt32(row["StocCurent"])
                });
            }

            return list;
        }

        public string BuildReportText()
        {
            var items = GetRaportItems();
            var totalCost = Convert.ToDecimal(ExecuteScalar("SELECT COALESCE(SUM(Cantitate * PretAchizitie), 0) FROM Aprovizionari") ?? 0m, CultureInfo.InvariantCulture);
            var lowStock = GetMedicamente().Where(m => m.StocCurent < 10).ToList();
            var topSupplier = ExecuteQuery(@"
                SELECT f.Denumire, COUNT(*) AS Total
                FROM Aprovizionari a
                JOIN Furnizori f ON f.IdFurnizor = a.IdFurnizor
                GROUP BY f.IdFurnizor, f.Denumire
                ORDER BY Total DESC, f.Denumire
                LIMIT 1");

            var sb = new StringBuilder();
            sb.AppendLine("Отчет по поставкам лекарств");
            sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine();
            sb.AppendLine("По каждому лекарству:");

            if (items.Count == 0)
            {
                sb.AppendLine("Нет данных.");
            }
            else
            {
                foreach (var item in items)
                {
                    sb.AppendLine(
                        $"{item.MedicamentDenumire} | {item.FurnizorDenumire} | поставок: {item.NumarAprovizionari} | " +
                        $"количество: {item.CantitateTotala} | сумма: {item.CostTotal:F2} | остаток: {item.StocCurent}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"Общая стоимость поставок: {totalCost:F2}");
            sb.AppendLine("Лекарства с низким остатком:");

            if (lowStock.Count == 0)
            {
                sb.AppendLine("нет");
            }
            else
            {
                foreach (var item in lowStock)
                {
                    sb.AppendLine($"{item.Denumire} ({item.StocCurent})");
                }
            }

            sb.AppendLine();
            if (topSupplier.Rows.Count > 0)
            {
                sb.AppendLine($"Поставщик с наибольшим числом поставок: {topSupplier.Rows[0]["Denumire"]} ({topSupplier.Rows[0]["Total"]})");
            }
            else
            {
                sb.AppendLine("Поставщик с наибольшим числом поставок: нет данных");
            }

            return sb.ToString().TrimEnd();
        }

        public void SaveMedicament(Medicament medicament, bool isEdit)
        {
            var denumire = Normalize(medicament.Denumire);
            var forma = Normalize(medicament.FormaFarmaceutica);
            var concentratie = Normalize(medicament.Concentratie);

            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();

            var duplicateQuery = @"
                SELECT COUNT(*)
                FROM Medicamente
                WHERE LOWER(TRIM(Denumire)) = LOWER(@denumire)
                  AND LOWER(TRIM(FormaFarmaceutica)) = LOWER(@forma)
                  AND LOWER(TRIM(Concentratie)) = LOWER(@concentratie)";
            var duplicateParams = new List<SqliteParameter>
            {
                new("@denumire", denumire),
                new("@forma", forma),
                new("@concentratie", concentratie)
            };

            if (isEdit)
            {
                duplicateQuery += " AND IdMedicament <> @id";
                duplicateParams.Add(new SqliteParameter("@id", medicament.IdMedicament));
            }

            var duplicateCount = Convert.ToInt32(ExecuteScalar(conn, tx, duplicateQuery, [.. duplicateParams]) ?? 0);
            if (duplicateCount > 0)
            {
                throw new InvalidOperationException("Лекарство с таким названием, формой и концентрацией уже существует.");
            }

            if (isEdit)
            {
                ExecuteNonQuery(conn, tx,
                    @"UPDATE Medicamente
                      SET Denumire = @denumire, FormaFarmaceutica = @forma, Concentratie = @concentratie, Pret = @pret
                      WHERE IdMedicament = @id",
                    new SqliteParameter("@denumire", denumire),
                    new SqliteParameter("@forma", forma),
                    new SqliteParameter("@concentratie", concentratie),
                    new SqliteParameter("@pret", medicament.Pret),
                    new SqliteParameter("@id", medicament.IdMedicament));
            }
            else
            {
                ExecuteNonQuery(conn, tx,
                    @"INSERT INTO Medicamente (Denumire, FormaFarmaceutica, Concentratie, Pret, StocCurent)
                      VALUES (@denumire, @forma, @concentratie, @pret, @stoc)",
                    new SqliteParameter("@denumire", denumire),
                    new SqliteParameter("@forma", forma),
                    new SqliteParameter("@concentratie", concentratie),
                    new SqliteParameter("@pret", medicament.Pret),
                    new SqliteParameter("@stoc", Math.Max(0, medicament.StocCurent)));
            }

            tx.Commit();
        }

        public void DeleteMedicament(int id)
        {
            ExecuteNonQuery("DELETE FROM Medicamente WHERE IdMedicament = @id", new SqliteParameter("@id", id));
        }

        public void SaveFurnizor(Furnizor furnizor, bool isEdit)
        {
            var denumire = Normalize(furnizor.Denumire);
            var telefon = Normalize(furnizor.Telefon);
            var email = Normalize(furnizor.Email);
            var adresa = Normalize(furnizor.Adresa);

            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new InvalidOperationException("Email поставщика имеет неверный формат.");
            }

            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();

            var duplicateCount = Convert.ToInt32(ExecuteScalar(conn, tx,
                @"SELECT COUNT(*) FROM Furnizori WHERE LOWER(TRIM(Email)) = LOWER(@email)" +
                (isEdit ? " AND IdFurnizor <> @id" : string.Empty),
                isEdit
                    ? [new SqliteParameter("@email", email), new SqliteParameter("@id", furnizor.IdFurnizor)]
                    : [new SqliteParameter("@email", email)]) ?? 0);

            if (duplicateCount > 0)
            {
                throw new InvalidOperationException("Поставщик с таким email уже существует.");
            }

            if (isEdit)
            {
                ExecuteNonQuery(conn, tx,
                    @"UPDATE Furnizori
                      SET Denumire = @denumire, Telefon = @telefon, Email = @email, Adresa = @adresa
                      WHERE IdFurnizor = @id",
                    new SqliteParameter("@denumire", denumire),
                    new SqliteParameter("@telefon", telefon),
                    new SqliteParameter("@email", email),
                    new SqliteParameter("@adresa", adresa),
                    new SqliteParameter("@id", furnizor.IdFurnizor));
            }
            else
            {
                ExecuteNonQuery(conn, tx,
                    @"INSERT INTO Furnizori (Denumire, Telefon, Email, Adresa)
                      VALUES (@denumire, @telefon, @email, @adresa)",
                    new SqliteParameter("@denumire", denumire),
                    new SqliteParameter("@telefon", telefon),
                    new SqliteParameter("@email", email),
                    new SqliteParameter("@adresa", adresa));
            }

            tx.Commit();
        }

        public void DeleteFurnizor(int id)
        {
            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();

            var dt = ExecuteQuery(conn, tx, @"
                SELECT IdMedicament, SUM(Cantitate) AS CantitateTotala
                FROM Aprovizionari
                WHERE IdFurnizor = @id
                GROUP BY IdMedicament",
                new SqliteParameter("@id", id));

            foreach (DataRow row in dt.Rows)
            {
                var medicamentId = Convert.ToInt32(row["IdMedicament"]);
                var qty = Convert.ToInt32(row["CantitateTotala"]);
                AdjustStock(conn, tx, medicamentId, -qty);
            }

            ExecuteNonQuery(conn, tx, "DELETE FROM Furnizori WHERE IdFurnizor = @id", new SqliteParameter("@id", id));
            tx.Commit();
        }

        public void SaveAprovizionare(Aprovizionare aprovizionare, bool isEdit)
        {
            if (aprovizionare.DataExpirare.Date <= aprovizionare.DataAprovizionare.Date)
            {
                throw new InvalidOperationException("Дата истечения срока годности должна быть позже даты поставки.");
            }

            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();

            if (isEdit)
            {
                var old = GetAprovizionareById(conn, tx, aprovizionare.IdAprovizionare)
                    ?? throw new InvalidOperationException("Выбранная поставка не найдена.");

                if (old.IdMedicament == aprovizionare.IdMedicament)
                {
                    AdjustStock(conn, tx, aprovizionare.IdMedicament, aprovizionare.Cantitate - old.Cantitate);
                }
                else
                {
                    AdjustStock(conn, tx, old.IdMedicament, -old.Cantitate);
                    AdjustStock(conn, tx, aprovizionare.IdMedicament, aprovizionare.Cantitate);
                }

                ExecuteNonQuery(conn, tx,
                    @"UPDATE Aprovizionari
                      SET IdMedicament = @idMedicament, IdFurnizor = @idFurnizor, DataAprovizionare = @dataAprovizionare,
                          Cantitate = @cantitate, PretAchizitie = @pretAchizitie, DataExpirare = @dataExpirare
                      WHERE IdAprovizionare = @id",
                    new SqliteParameter("@idMedicament", aprovizionare.IdMedicament),
                    new SqliteParameter("@idFurnizor", aprovizionare.IdFurnizor),
                    new SqliteParameter("@dataAprovizionare", aprovizionare.DataAprovizionare.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new SqliteParameter("@cantitate", aprovizionare.Cantitate),
                    new SqliteParameter("@pretAchizitie", aprovizionare.PretAchizitie),
                    new SqliteParameter("@dataExpirare", aprovizionare.DataExpirare.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new SqliteParameter("@id", aprovizionare.IdAprovizionare));
            }
            else
            {
                AdjustStock(conn, tx, aprovizionare.IdMedicament, aprovizionare.Cantitate);
                ExecuteNonQuery(conn, tx,
                    @"INSERT INTO Aprovizionari (IdMedicament, IdFurnizor, DataAprovizionare, Cantitate, PretAchizitie, DataExpirare)
                      VALUES (@idMedicament, @idFurnizor, @dataAprovizionare, @cantitate, @pretAchizitie, @dataExpirare)",
                    new SqliteParameter("@idMedicament", aprovizionare.IdMedicament),
                    new SqliteParameter("@idFurnizor", aprovizionare.IdFurnizor),
                    new SqliteParameter("@dataAprovizionare", aprovizionare.DataAprovizionare.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    new SqliteParameter("@cantitate", aprovizionare.Cantitate),
                    new SqliteParameter("@pretAchizitie", aprovizionare.PretAchizitie),
                    new SqliteParameter("@dataExpirare", aprovizionare.DataExpirare.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }

            tx.Commit();
        }

        public void DeleteAprovizionare(int id)
        {
            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();
            var old = GetAprovizionareById(conn, tx, id) ?? throw new InvalidOperationException("Выбранная поставка не найдена.");
            AdjustStock(conn, tx, old.IdMedicament, -old.Cantitate);
            ExecuteNonQuery(conn, tx, "DELETE FROM Aprovizionari WHERE IdAprovizionare = @id", new SqliteParameter("@id", id));
            tx.Commit();
        }

        private void InitializeDatabase()
        {
            using var conn = OpenConnection();

            ExecuteNonQuery(conn, null, @"
                CREATE TABLE IF NOT EXISTS Medicamente (
                    IdMedicament INTEGER PRIMARY KEY AUTOINCREMENT,
                    Denumire TEXT NOT NULL,
                    FormaFarmaceutica TEXT NOT NULL,
                    Concentratie TEXT NOT NULL,
                    Pret REAL NOT NULL CHECK(Pret > 0),
                    StocCurent INTEGER NOT NULL CHECK(StocCurent >= 0),
                    UNIQUE(Denumire, FormaFarmaceutica, Concentratie)
                );");

            ExecuteNonQuery(conn, null, @"
                CREATE TABLE IF NOT EXISTS Furnizori (
                    IdFurnizor INTEGER PRIMARY KEY AUTOINCREMENT,
                    Denumire TEXT NOT NULL,
                    Telefon TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    Adresa TEXT NOT NULL
                );");

            ExecuteNonQuery(conn, null, @"
                CREATE TABLE IF NOT EXISTS Aprovizionari (
                    IdAprovizionare INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdMedicament INTEGER NOT NULL,
                    IdFurnizor INTEGER NOT NULL,
                    DataAprovizionare TEXT NOT NULL,
                    Cantitate INTEGER NOT NULL CHECK(Cantitate > 0),
                    PretAchizitie REAL NOT NULL CHECK(PretAchizitie > 0),
                    DataExpirare TEXT NOT NULL,
                    FOREIGN KEY(IdMedicament) REFERENCES Medicamente(IdMedicament) ON DELETE CASCADE,
                    FOREIGN KEY(IdFurnizor) REFERENCES Furnizori(IdFurnizor) ON DELETE CASCADE
                );");

            SeedTestDataIfEmpty(conn);
        }

        private void SeedTestDataIfEmpty(SqliteConnection conn)
        {
            var count = Convert.ToInt32(ExecuteScalar(conn, null, "SELECT COUNT(*) FROM Medicamente") ?? 0);
            if (count > 0)
            {
                return;
            }

            using var tx = conn.BeginTransaction();

            var medicamente = new[]
            {
                ("Paracetamol", "comprimat", "500 mg", 4.50m),
                ("Ibuprofen", "capsula", "200 mg", 6.20m),
                ("Amoxicilina", "capsula", "250 mg", 8.40m),
                ("Cetirizina", "tableta", "10 mg", 5.10m),
                ("Omeprazol", "capsula", "20 mg", 12.00m),
                ("Vitamina C", "comprimat efervescent", "1000 mg", 7.80m),
                ("Salbutamol", "spray", "0,1 mg", 18.50m),
                ("Metformin", "comprimat", "850 mg", 9.90m)
            };

            foreach (var medicament in medicamente)
            {
                ExecuteNonQuery(conn, tx,
                    @"INSERT INTO Medicamente (Denumire, FormaFarmaceutica, Concentratie, Pret, StocCurent)
                      VALUES (@denumire, @forma, @concentratie, @pret, 0)",
                    new SqliteParameter("@denumire", medicament.Item1),
                    new SqliteParameter("@forma", medicament.Item2),
                    new SqliteParameter("@concentratie", medicament.Item3),
                    new SqliteParameter("@pret", medicament.Item4));
            }

            var furnizori = new[]
            {
                ("Farmex SRL", "021-100-200", "contact@farmex.md", "Str. Independentei 1"),
                ("MediPlus", "022-200-300", "office@mediplus.md", "Str. Libertatii 12"),
                ("Pharma Nord", "023-300-400", "sales@pharmanord.md", "Bd. Stefan cel Mare 45"),
                ("SanaMarket", "024-400-500", "info@sanamarket.md", "Str. Bucuresti 8"),
                ("GreenHealth", "025-500-600", "hello@greenhealth.md", "Str. Testului 19")
            };

            foreach (var furnizor in furnizori)
            {
                ExecuteNonQuery(conn, tx,
                    @"INSERT INTO Furnizori (Denumire, Telefon, Email, Adresa)
                      VALUES (@denumire, @telefon, @email, @adresa)",
                    new SqliteParameter("@denumire", furnizor.Item1),
                    new SqliteParameter("@telefon", furnizor.Item2),
                    new SqliteParameter("@email", furnizor.Item3),
                    new SqliteParameter("@adresa", furnizor.Item4));
            }

            var deliveries = new (int medicamentId, int furnizorId, string data, int cantitate, decimal pret, string expirare)[]
            {
                (1, 1, "2026-01-10", 10, 3.80m, "2028-01-10"),
                (2, 2, "2026-01-11", 5, 5.50m, "2027-12-31"),
                (3, 3, "2026-01-12", 12, 7.90m, "2027-11-20"),
                (4, 4, "2026-01-13", 7, 4.70m, "2028-02-01"),
                (5, 5, "2026-01-14", 10, 10.80m, "2027-10-15"),
                (6, 1, "2026-01-15", 15, 6.90m, "2027-12-01"),
                (7, 2, "2026-01-16", 6, 16.40m, "2027-09-01"),
                (8, 3, "2026-01-17", 20, 8.70m, "2027-08-15"),
                (1, 4, "2026-01-18", 8, 3.90m, "2028-01-10"),
                (2, 5, "2026-01-19", 4, 5.30m, "2027-12-31"),
                (3, 1, "2026-01-20", 6, 7.70m, "2027-11-20"),
                (4, 2, "2026-01-21", 5, 4.60m, "2028-02-01"),
                (5, 3, "2026-01-22", 3, 11.20m, "2027-10-15"),
                (6, 4, "2026-01-23", 5, 6.70m, "2027-12-01"),
                (7, 5, "2026-01-24", 4, 15.90m, "2027-09-01")
            };

            foreach (var delivery in deliveries)
            {
                ExecuteNonQuery(conn, tx,
                    @"INSERT INTO Aprovizionari (IdMedicament, IdFurnizor, DataAprovizionare, Cantitate, PretAchizitie, DataExpirare)
                      VALUES (@medicamentId, @furnizorId, @data, @cantitate, @pret, @expirare)",
                    new SqliteParameter("@medicamentId", delivery.medicamentId),
                    new SqliteParameter("@furnizorId", delivery.furnizorId),
                    new SqliteParameter("@data", delivery.data),
                    new SqliteParameter("@cantitate", delivery.cantitate),
                    new SqliteParameter("@pret", delivery.pret),
                    new SqliteParameter("@expirare", delivery.expirare));

                AdjustStock(conn, tx, delivery.medicamentId, delivery.cantitate);
            }

            tx.Commit();
        }

        private Aprovizionare? GetAprovizionareById(SqliteConnection conn, SqliteTransaction tx, int id)
        {
            var dt = ExecuteQuery(conn, tx, @"
                SELECT IdAprovizionare, IdMedicament, IdFurnizor, DataAprovizionare, Cantitate, PretAchizitie, DataExpirare
                FROM Aprovizionari
                WHERE IdAprovizionare = @id",
                new SqliteParameter("@id", id));

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            var row = dt.Rows[0];
            return new Aprovizionare
            {
                IdAprovizionare = Convert.ToInt32(row["IdAprovizionare"]),
                IdMedicament = Convert.ToInt32(row["IdMedicament"]),
                IdFurnizor = Convert.ToInt32(row["IdFurnizor"]),
                DataAprovizionare = DateTime.Parse(row["DataAprovizionare"].ToString() ?? string.Empty, CultureInfo.InvariantCulture),
                Cantitate = Convert.ToInt32(row["Cantitate"]),
                PretAchizitie = Convert.ToDecimal(row["PretAchizitie"], CultureInfo.InvariantCulture),
                DataExpirare = DateTime.Parse(row["DataExpirare"].ToString() ?? string.Empty, CultureInfo.InvariantCulture)
            };
        }

        private void AdjustStock(SqliteConnection conn, SqliteTransaction tx, int medicamentId, int delta)
        {
            var current = Convert.ToInt32(ExecuteScalar(conn, tx, "SELECT StocCurent FROM Medicamente WHERE IdMedicament = @id", new SqliteParameter("@id", medicamentId)) ?? 0);
            var next = current + delta;
            if (next < 0)
            {
                var name = Convert.ToString(ExecuteScalar(conn, tx, "SELECT Denumire FROM Medicamente WHERE IdMedicament = @id", new SqliteParameter("@id", medicamentId))) ?? "лекарство";
                throw new InvalidOperationException($"Остаток для \"{name}\" не может стать отрицательным.");
            }

            ExecuteNonQuery(conn, tx,
                "UPDATE Medicamente SET StocCurent = @stoc WHERE IdMedicament = @id",
                new SqliteParameter("@stoc", next),
                new SqliteParameter("@id", medicamentId));
        }

        private SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var pragma = new SqliteCommand("PRAGMA foreign_keys = ON;", conn);
            pragma.ExecuteNonQuery();

            return conn;
        }

        private static SqliteCommand CreateCommand(SqliteConnection conn, string query, params SqliteParameter[] parameters)
        {
            var cmd = new SqliteCommand(query, conn);
            if (parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd;
        }

        private static SqliteCommand CreateCommand(SqliteConnection conn, SqliteTransaction? tx, string query, params SqliteParameter[] parameters)
        {
            var cmd = new SqliteCommand(query, conn, tx);
            if (parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd;
        }

        private static DataTable ExecuteQuery(SqliteConnection conn, SqliteTransaction? tx, string query, params SqliteParameter[] parameters)
        {
            using var cmd = CreateCommand(conn, tx, query, parameters);
            using var reader = cmd.ExecuteReader();
            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        private static int ExecuteNonQuery(SqliteConnection conn, SqliteTransaction? tx, string query, params SqliteParameter[] parameters)
        {
            using var cmd = CreateCommand(conn, tx, query, parameters);
            return cmd.ExecuteNonQuery();
        }

        private static object? ExecuteScalar(SqliteConnection conn, SqliteTransaction? tx, string query, params SqliteParameter[] parameters)
        {
            using var cmd = CreateCommand(conn, tx, query, parameters);
            return cmd.ExecuteScalar();
        }

        private static string Normalize(string value) => value.Trim();
    }
}
