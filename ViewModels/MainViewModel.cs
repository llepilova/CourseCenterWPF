using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using CourseCenterWPF.Views.DialogWindows;

namespace CourseCenterWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db = new DatabaseService();

        public ObservableCollection<Cursant> Cursanti { get; set; }
        public ObservableCollection<Curs> Cursuri { get; set; }
        public ObservableCollection<Inscriere> Inscrieri { get; set; }
        public ObservableCollection<Cursant> CursantiForCombo { get; set; }
        public ObservableCollection<Curs> CursuriForCombo { get; set; }

        private Cursant _selectedCursant;
        public Cursant SelectedCursant
        {
            get => _selectedCursant;
            set { _selectedCursant = value; OnPropertyChanged(); }
        }

        private Curs _selectedCurs;
        public Curs SelectedCurs
        {
            get => _selectedCurs;
            set { _selectedCurs = value; OnPropertyChanged(); }
        }

        private Inscriere _selectedInscriere;
        public Inscriere SelectedInscriere
        {
            get => _selectedInscriere;
            set { _selectedInscriere = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); SearchCursanti(); }
        }

        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; OnPropertyChanged(); FilterCursuri(); }
        }

        private string _statisticsText;
        public string StatisticsText
        {
            get => _statisticsText;
            set { _statisticsText = value; OnPropertyChanged(); }
        }

        private DataTable _raportData;
        public DataTable RaportData
        {
            get => _raportData;
            set { _raportData = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCursantCommand { get; set; }
        public RelayCommand EditCursantCommand { get; set; }
        public RelayCommand DeleteCursantCommand { get; set; }
        public RelayCommand AddCursCommand { get; set; }
        public RelayCommand EditCursCommand { get; set; }
        public RelayCommand DeleteCursCommand { get; set; }
        public RelayCommand AddInscriereCommand { get; set; }
        public RelayCommand DeleteInscriereCommand { get; set; }
        public RelayCommand ExportRaportCommand { get; set; }

        public MainViewModel()
        {
            Cursanti = new ObservableCollection<Cursant>();
            Cursuri = new ObservableCollection<Curs>();
            Inscrieri = new ObservableCollection<Inscriere>();
            CursantiForCombo = new ObservableCollection<Cursant>();
            CursuriForCombo = new ObservableCollection<Curs>();

            LoadAllData();

            AddCursantCommand = new RelayCommand(AddCursant);
            EditCursantCommand = new RelayCommand(EditCursant, () => SelectedCursant != null);
            DeleteCursantCommand = new RelayCommand(DeleteCursant, () => SelectedCursant != null);
            AddCursCommand = new RelayCommand(AddCurs);
            EditCursCommand = new RelayCommand(EditCurs, () => SelectedCurs != null);
            DeleteCursCommand = new RelayCommand(DeleteCurs, () => SelectedCurs != null);
            AddInscriereCommand = new RelayCommand(AddInscriere);
            DeleteInscriereCommand = new RelayCommand(DeleteInscriere, () => SelectedInscriere != null);
            ExportRaportCommand = new RelayCommand(ExportRaport);
        }

        private void LoadAllData()
        {
            LoadCursanti();
            LoadCursuri();
            LoadInscrieri();
            LoadRaport();
            LoadComboBoxData();
        }

        private void LoadCursanti()
        {
            Cursanti.Clear();
            foreach (var item in _db.GetCursanti())
                Cursanti.Add(item);
        }

        private void LoadCursuri()
        {
            Cursuri.Clear();
            foreach (var item in _db.GetCursuri())
                Cursuri.Add(item);
        }

        private void LoadInscrieri()
        {
            Inscrieri.Clear();
            string query = @"
                SELECT i.IdInscriere, i.IdCursant, i.IdCurs, i.DataInscriere, i.StatusPlata,
                       CONCAT(c.Nume, ' ', c.Prenume) AS NumeCursant,
                       cu.Denumire AS DenumireCurs
                FROM Inscriere i
                JOIN Cursant c ON i.IdCursant = c.IdCursant
                JOIN Curs cu ON i.IdCurs = cu.IdCurs
                ORDER BY i.DataInscriere DESC";
            DataTable dt = _db.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                Inscrieri.Add(new Inscriere
                {
                    IdInscriere = Convert.ToInt32(row["IdInscriere"]),
                    IdCursant = Convert.ToInt32(row["IdCursant"]),
                    IdCurs = Convert.ToInt32(row["IdCurs"]),
                    DataInscriere = Convert.ToDateTime(row["DataInscriere"]),
                    StatusPlata = row["StatusPlata"].ToString(),
                    NumeCursant = row["NumeCursant"].ToString(),
                    DenumireCurs = row["DenumireCurs"].ToString()
                });
            }
        }

        private void LoadComboBoxData()
        {
            CursantiForCombo.Clear();
            foreach (var c in _db.GetCursanti())
                CursantiForCombo.Add(c);

            CursuriForCombo.Clear();
            foreach (var c in _db.GetCursuri())
                CursuriForCombo.Add(c);
        }

        private void LoadRaport()
        {
            string query = @"
                SELECT 
                    CONCAT(c.Nume, ' ', c.Prenume) AS FullName,
                    COUNT(i.IdInscriere) AS Registrations,
                    SUM(CASE WHEN i.StatusPlata = 'Platit' THEN cu.Pret ELSE 0 END) AS TotalPaid
                FROM Cursant c
                LEFT JOIN Inscriere i ON c.IdCursant = i.IdCursant
                LEFT JOIN Curs cu ON i.IdCurs = cu.IdCurs
                GROUP BY c.IdCursant
                ORDER BY TotalPaid DESC;";
            RaportData = _db.ExecuteQuery(query);

            string statsQuery = @"
                SELECT 
                    (SELECT COUNT(*) FROM Cursant) AS TotalCursanti,
                    (SELECT COALESCE(SUM(Pret), 0) FROM Inscriere i JOIN Curs cu ON i.IdCurs = cu.IdCurs WHERE StatusPlata = 'Platit') AS TotalIncasari,
                    (SELECT COALESCE(AVG(TotalPaid), 0) FROM 
                        (SELECT SUM(Pret) AS TotalPaid FROM Inscriere i JOIN Curs cu ON i.IdCurs = cu.IdCurs WHERE StatusPlata = 'Platit' GROUP BY IdCursant) AS t) AS AvgPerCursant,
                    (SELECT cu.Denumire FROM Inscriere i JOIN Curs cu ON i.IdCurs = cu.IdCurs GROUP BY cu.IdCurs ORDER BY COUNT(*) DESC LIMIT 1) AS TopCurs;
                ";
            DataTable stats = _db.ExecuteQuery(statsQuery);
            if (stats.Rows.Count > 0)
            {
                StatisticsText = $"👥 Слушателей: {stats.Rows[0]["TotalCursanti"]} | " +
                                 $"💰 Поступлений: {Convert.ToDecimal(stats.Rows[0]["TotalIncasari"]):C} | " +
                                 $"📊 Среднее: {Convert.ToDecimal(stats.Rows[0]["AvgPerCursant"]):C} | " +
                                 $"🏆 Топ курс: {stats.Rows[0]["TopCurs"]}";
            }
        }

        private void SearchCursanti()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadCursanti();
                return;
            }
            Cursanti.Clear();
            string query = $"SELECT * FROM Cursant WHERE Nume LIKE '%{SearchText}%' OR Prenume LIKE '%{SearchText}%' OR Email LIKE '%{SearchText}%'";
            DataTable dt = _db.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                Cursanti.Add(new Cursant
                {
                    IdCursant = Convert.ToInt32(row["IdCursant"]),
                    Nume = row["Nume"].ToString(),
                    Prenume = row["Prenume"].ToString(),
                    Telefon = row["Telefon"].ToString(),
                    Email = row["Email"].ToString()
                });
            }
        }

        private void FilterCursuri()
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                LoadCursuri();
                return;
            }
            Cursuri.Clear();
            string query = $"SELECT * FROM Curs WHERE Formator LIKE '%{FilterText}%' OR DurataZile = '{FilterText}'";
            DataTable dt = _db.ExecuteQuery(query);
            foreach (DataRow row in dt.Rows)
            {
                Cursuri.Add(new Curs
                {
                    IdCurs = Convert.ToInt32(row["IdCurs"]),
                    Denumire = row["Denumire"].ToString(),
                    Formator = row["Formator"].ToString(),
                    Pret = Convert.ToDecimal(row["Pret"]),
                    DurataZile = Convert.ToInt32(row["DurataZile"])
                });
            }
        }

        private void AddCursant()
        {
            var dialog = new CursantDialog();
            if (dialog.ShowDialog() == true)
                LoadAllData();
        }

        private void EditCursant()
        {
            var dialog = new CursantDialog(SelectedCursant);
            if (dialog.ShowDialog() == true)
                LoadAllData();
        }

        private void DeleteCursant()
        {
            if (MessageBox.Show($"Удалить слушателя {SelectedCursant.FullName}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.ExecuteNonQuery($"DELETE FROM Cursant WHERE IdCursant = {SelectedCursant.IdCursant}");
                LoadAllData();
            }
        }

        private void AddCurs()
        {
            var dialog = new CursDialog();
            if (dialog.ShowDialog() == true)
                LoadAllData();
        }

        private void EditCurs()
        {
            var dialog = new CursDialog(SelectedCurs);
            if (dialog.ShowDialog() == true)
                LoadAllData();
        }

        private void DeleteCurs()
        {
            if (MessageBox.Show($"Удалить курс {SelectedCurs.Denumire}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.ExecuteNonQuery($"DELETE FROM Curs WHERE IdCurs = {SelectedCurs.IdCurs}");
                LoadAllData();
            }
        }

        private void AddInscriere()
        {
            var dialog = new InscriereDialog(CursantiForCombo, CursuriForCombo);
            if (dialog.ShowDialog() == true)
                LoadAllData();
        }

        private void DeleteInscriere()
        {
            if (MessageBox.Show($"Отменить регистрацию на курс {SelectedInscriere.DenumireCurs}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.ExecuteNonQuery($"DELETE FROM Inscriere WHERE IdInscriere = {SelectedInscriere.IdInscriere}");
                LoadAllData();
            }
        }

        private void ExportRaport()
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    string content = "========== ОТЧЕТ ПО УЧАСТИЮ И ОПЛАТАМ ==========\n\n";
                    content += "Слушатель | Регистраций | Оплачено\n";
                    content += "----------------------------------------\n";

                    foreach (DataRow row in RaportData.Rows)
                    {
                        content += $"{row["FullName"]} | {row["Registrations"]} | {Convert.ToDecimal(row["TotalPaid"]):C}\n";
                    }

                    content += $"\n{StatisticsText}";
                    content += "\n\n===========================================";

                    System.IO.File.WriteAllText(sfd.FileName, content);
                    MessageBox.Show("Отчет успешно экспортирован!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}