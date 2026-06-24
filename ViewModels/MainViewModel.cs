using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using CourseCenterWPF.Views.DialogWindows;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CourseCenterWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db = new DatabaseService();

        public ObservableCollection<Film> Filme { get; } = new();
        public ObservableCollection<Seans> Seanse { get; } = new();
        public ObservableCollection<Bilet> Bilete { get; } = new();
        public ObservableCollection<Film> FilmeForCombo { get; } = new();
        public ObservableCollection<Seans> SeanseForCombo { get; } = new();

        private Film? _selectedFilm;
        public Film? SelectedFilm
        {
            get => _selectedFilm;
            set { _selectedFilm = value; OnPropertyChanged(); }
        }

        private Seans? _selectedSeans;
        public Seans? SelectedSeans
        {
            get => _selectedSeans;
            set { _selectedSeans = value; OnPropertyChanged(); }
        }

        private Bilet? _selectedBilet;
        public Bilet? SelectedBilet
        {
            get => _selectedBilet;
            set { _selectedBilet = value; OnPropertyChanged(); }
        }

        private string _filmSearchText = string.Empty;
        public string FilmSearchText
        {
            get => _filmSearchText;
            set
            {
                _filmSearchText = value;
                OnPropertyChanged();
                LoadFilme();
            }
        }

        private Film? _selectedFilmFilter;
        public Film? SelectedFilmFilter
        {
            get => _selectedFilmFilter;
            set
            {
                _selectedFilmFilter = value;
                OnPropertyChanged();
                LoadSeanse();
            }
        }

        private DateTime? _seansDateFilter;
        public DateTime? SeansDateFilter
        {
            get => _seansDateFilter;
            set
            {
                _seansDateFilter = value;
                OnPropertyChanged();
                LoadSeanse();
            }
        }

        private Film? _selectedFilmBiletFilter;
        public Film? SelectedFilmBiletFilter
        {
            get => _selectedFilmBiletFilter;
            set
            {
                _selectedFilmBiletFilter = value;
                OnPropertyChanged();
                LoadBilete();
            }
        }

        private Seans? _selectedSeansBiletFilter;
        public Seans? SelectedSeansBiletFilter
        {
            get => _selectedSeansBiletFilter;
            set
            {
                _selectedSeansBiletFilter = value;
                OnPropertyChanged();
                LoadBilete();
            }
        }

        private DateTime? _biletDateFilter;
        public DateTime? BiletDateFilter
        {
            get => _biletDateFilter;
            set
            {
                _biletDateFilter = value;
                OnPropertyChanged();
                LoadBilete();
            }
        }

        public RelayCommand AddFilmCommand { get; }
        public RelayCommand EditFilmCommand { get; }
        public RelayCommand DeleteFilmCommand { get; }
        public RelayCommand AddSeansCommand { get; }
        public RelayCommand EditSeansCommand { get; }
        public RelayCommand DeleteSeansCommand { get; }
        public RelayCommand AddBiletCommand { get; }
        public RelayCommand EditBiletCommand { get; }
        public RelayCommand DeleteBiletCommand { get; }
        public RelayCommand ResetSeansFiltersCommand { get; }
        public RelayCommand ResetBiletFiltersCommand { get; }

        public MainViewModel()
        {
            AddFilmCommand = new RelayCommand(AddFilm);
            EditFilmCommand = new RelayCommand(EditFilm, () => SelectedFilm != null);
            DeleteFilmCommand = new RelayCommand(DeleteFilm, () => SelectedFilm != null);
            AddSeansCommand = new RelayCommand(AddSeans);
            EditSeansCommand = new RelayCommand(EditSeans, () => SelectedSeans != null);
            DeleteSeansCommand = new RelayCommand(DeleteSeans, () => SelectedSeans != null);
            AddBiletCommand = new RelayCommand(AddBilet);
            EditBiletCommand = new RelayCommand(EditBilet, () => SelectedBilet != null);
            DeleteBiletCommand = new RelayCommand(DeleteBilet, () => SelectedBilet != null);
            ResetSeansFiltersCommand = new RelayCommand(ResetSeansFilters);
            ResetBiletFiltersCommand = new RelayCommand(ResetBiletFilters);

            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadFilmeForCombo();
            LoadFilme();
            LoadSeanseForCombo();
            LoadSeanse();
            LoadBilete();
        }

        private void LoadFilmeForCombo()
        {
            FilmeForCombo.Clear();
            FilmeForCombo.Add(new Film { IdFilm = 0, Titlu = "Все фильмы" });
            foreach (var film in _db.GetFilme())
            {
                FilmeForCombo.Add(film);
            }

            SelectedFilmFilter ??= FilmeForCombo[0];
            SelectedFilmBiletFilter ??= FilmeForCombo[0];
        }

        private void LoadSeanseForCombo()
        {
            SeanseForCombo.Clear();
            SeanseForCombo.Add(new Seans { IdSeansa = 0, FilmTitlu = "Все сеансы", DataSeansa = DateTime.Today, OraSeansa = "-" });

            var dt = _db.ExecuteQuery(@"
                SELECT s.IdSeansa, f.Titlu, s.DataSeansa, s.OraSeansa
                FROM Seanse s
                JOIN Filme f ON f.IdFilm = s.IdFilm
                ORDER BY s.DataSeansa, s.OraSeansa");

            foreach (DataRow row in dt.Rows)
            {
                SeanseForCombo.Add(new Seans
                {
                    IdSeansa = Convert.ToInt32(row["IdSeansa"]),
                    FilmTitlu = row["Titlu"].ToString() ?? string.Empty,
                    DataSeansa = DateTime.Parse(row["DataSeansa"].ToString() ?? DateTime.Today.ToString("yyyy-MM-dd")),
                    OraSeansa = row["OraSeansa"].ToString() ?? string.Empty
                });
            }

            SelectedSeansBiletFilter ??= SeanseForCombo[0];
        }

        private void LoadFilme()
        {
            Filme.Clear();
            var query = "SELECT IdFilm, Titlu, Gen, DurataMinute, LimitaVarsta FROM Filme";
            var parameters = new Collection<SqliteParameter>();

            if (!string.IsNullOrWhiteSpace(FilmSearchText))
            {
                query += " WHERE Titlu LIKE @q OR Gen LIKE @q OR CAST(LimitaVarsta AS TEXT) LIKE @q";
                parameters.Add(new SqliteParameter("@q", $"%{FilmSearchText.Trim()}%"));
            }

            query += " ORDER BY Titlu";
            var dt = _db.ExecuteQuery(query, [.. parameters]);
            foreach (DataRow row in dt.Rows)
            {
                Filme.Add(new Film
                {
                    IdFilm = Convert.ToInt32(row["IdFilm"]),
                    Titlu = row["Titlu"].ToString() ?? string.Empty,
                    Gen = row["Gen"].ToString() ?? string.Empty,
                    DurataMinute = Convert.ToInt32(row["DurataMinute"]),
                    LimitaVarsta = Convert.ToInt32(row["LimitaVarsta"])
                });
            }
        }

        private void LoadSeanse()
        {
            Seanse.Clear();
            var query = @"
                SELECT s.IdSeansa, s.IdFilm, f.Titlu, s.DataSeansa, s.OraSeansa, s.PretBilet, s.NumarLocuriTotal,
                       COALESCE(SUM(b.NumarBilete), 0) AS TotalBileteVandute
                FROM Seanse s
                JOIN Filme f ON f.IdFilm = s.IdFilm
                LEFT JOIN Bilete b ON b.IdSeansa = s.IdSeansa
                WHERE 1=1";
            var parameters = new Collection<SqliteParameter>();

            if (SelectedFilmFilter is { IdFilm: > 0 })
            {
                query += " AND s.IdFilm = @filmId";
                parameters.Add(new SqliteParameter("@filmId", SelectedFilmFilter.IdFilm));
            }

            if (SeansDateFilter.HasValue)
            {
                query += " AND s.DataSeansa = @data";
                parameters.Add(new SqliteParameter("@data", SeansDateFilter.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }

            query += " GROUP BY s.IdSeansa ORDER BY s.DataSeansa, s.OraSeansa";

            var dt = _db.ExecuteQuery(query, [.. parameters]);
            foreach (DataRow row in dt.Rows)
            {
                Seanse.Add(new Seans
                {
                    IdSeansa = Convert.ToInt32(row["IdSeansa"]),
                    IdFilm = Convert.ToInt32(row["IdFilm"]),
                    FilmTitlu = row["Titlu"].ToString() ?? string.Empty,
                    DataSeansa = DateTime.Parse(row["DataSeansa"].ToString() ?? DateTime.Today.ToString("yyyy-MM-dd")),
                    OraSeansa = row["OraSeansa"].ToString() ?? string.Empty,
                    PretBilet = Convert.ToDecimal(row["PretBilet"]),
                    NumarLocuriTotal = Convert.ToInt32(row["NumarLocuriTotal"]),
                    TotalBileteVandute = Convert.ToInt32(row["TotalBileteVandute"])
                });
            }
        }

        private void LoadBilete()
        {
            Bilete.Clear();
            var query = @"
                SELECT b.IdBilet, b.IdSeansa, b.ReducereProcent, b.NumarBilete, b.SumaAchitata, b.DataVanzare,
                       f.Titlu, s.DataSeansa, s.OraSeansa
                FROM Bilete b
                JOIN Seanse s ON s.IdSeansa = b.IdSeansa
                JOIN Filme f ON f.IdFilm = s.IdFilm
                WHERE 1=1";
            var parameters = new Collection<SqliteParameter>();

            if (SelectedFilmBiletFilter is { IdFilm: > 0 })
            {
                query += " AND f.IdFilm = @filmId";
                parameters.Add(new SqliteParameter("@filmId", SelectedFilmBiletFilter.IdFilm));
            }

            if (SelectedSeansBiletFilter is { IdSeansa: > 0 })
            {
                query += " AND s.IdSeansa = @seansId";
                parameters.Add(new SqliteParameter("@seansId", SelectedSeansBiletFilter.IdSeansa));
            }

            if (BiletDateFilter.HasValue)
            {
                query += " AND b.DataVanzare = @data";
                parameters.Add(new SqliteParameter("@data", BiletDateFilter.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            }

            query += " ORDER BY b.DataVanzare DESC, b.IdBilet DESC";

            var dt = _db.ExecuteQuery(query, [.. parameters]);
            foreach (DataRow row in dt.Rows)
            {
                var dataSeansa = DateTime.Parse(row["DataSeansa"].ToString() ?? DateTime.Today.ToString("yyyy-MM-dd"));
                Bilete.Add(new Bilet
                {
                    IdBilet = Convert.ToInt32(row["IdBilet"]),
                    IdSeansa = Convert.ToInt32(row["IdSeansa"]),
                    ReducereProcent = Convert.ToDecimal(row["ReducereProcent"]),
                    NumarBilete = Convert.ToInt32(row["NumarBilete"]),
                    SumaAchitata = Convert.ToDecimal(row["SumaAchitata"]),
                    DataVanzare = DateTime.Parse(row["DataVanzare"].ToString() ?? DateTime.Today.ToString("yyyy-MM-dd")),
                    FilmTitlu = row["Titlu"].ToString() ?? string.Empty,
                    SeansInfo = $"{dataSeansa:dd.MM.yyyy} {row["OraSeansa"]}"
                });
            }
        }

        private void AddFilm()
        {
            var dialog = new FilmDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void EditFilm()
        {
            if (SelectedFilm == null)
            {
                return;
            }

            var dialog = new FilmDialog(SelectedFilm);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void DeleteFilm()
        {
            if (SelectedFilm == null)
            {
                return;
            }

            if (MessageBox.Show(
                    $"Удалить фильм \"{SelectedFilm.Titlu}\" и связанные сеансы/билеты?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _db.ExecuteNonQuery("DELETE FROM Filme WHERE IdFilm = @id", new SqliteParameter("@id", SelectedFilm.IdFilm));
            LoadAllData();
        }

        private void AddSeans()
        {
            var dialog = new SeansDialog(FilmeForCombo);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void EditSeans()
        {
            if (SelectedSeans == null)
            {
                return;
            }

            var dialog = new SeansDialog(FilmeForCombo, SelectedSeans);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void DeleteSeans()
        {
            if (SelectedSeans == null)
            {
                return;
            }

            if (MessageBox.Show(
                    "Удалить сеанс и связанные продажи билетов?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _db.ExecuteNonQuery("DELETE FROM Seanse WHERE IdSeansa = @id", new SqliteParameter("@id", SelectedSeans.IdSeansa));
            LoadAllData();
        }

        private void AddBilet()
        {
            var dialog = new BiletDialog(SeanseForCombo);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void EditBilet()
        {
            if (SelectedBilet == null)
            {
                return;
            }

            var dialog = new BiletDialog(SeanseForCombo, SelectedBilet);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void DeleteBilet()
        {
            if (SelectedBilet == null)
            {
                return;
            }

            if (MessageBox.Show(
                    "Удалить выбранную продажу билетов?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _db.ExecuteNonQuery("DELETE FROM Bilete WHERE IdBilet = @id", new SqliteParameter("@id", SelectedBilet.IdBilet));
            LoadAllData();
        }

        private void ResetSeansFilters()
        {
            SeansDateFilter = null;
            SelectedFilmFilter = FilmeForCombo.Count > 0 ? FilmeForCombo[0] : null;
            LoadSeanse();
        }

        private void ResetBiletFilters()
        {
            BiletDateFilter = null;
            SelectedFilmBiletFilter = FilmeForCombo.Count > 0 ? FilmeForCombo[0] : null;
            SelectedSeansBiletFilter = SeanseForCombo.Count > 0 ? SeanseForCombo[0] : null;
            LoadBilete();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
