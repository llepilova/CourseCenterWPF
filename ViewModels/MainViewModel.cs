using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using CourseCenterWPF.Views.DialogWindows;
using CourseCenterWPF.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CourseCenterWPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db = new();

        public ObservableCollection<Medicament> Medicamente { get; } = new();
        public ObservableCollection<Furnizor> Furnizori { get; } = new();
        public ObservableCollection<Aprovizionare> Aprovizionari { get; } = new();
        public ObservableCollection<Medicament> MedicamenteForFilter { get; } = new();
        public ObservableCollection<Furnizor> FurnizoriForFilter { get; } = new();

        private Medicament? _selectedMedicament;
        public Medicament? SelectedMedicament
        {
            get => _selectedMedicament;
            set { _selectedMedicament = value; OnPropertyChanged(); }
        }

        private Furnizor? _selectedFurnizor;
        public Furnizor? SelectedFurnizor
        {
            get => _selectedFurnizor;
            set { _selectedFurnizor = value; OnPropertyChanged(); }
        }

        private Aprovizionare? _selectedAprovizionare;
        public Aprovizionare? SelectedAprovizionare
        {
            get => _selectedAprovizionare;
            set { _selectedAprovizionare = value; OnPropertyChanged(); }
        }

        private string _medicamentSearchText = string.Empty;
        public string MedicamentSearchText
        {
            get => _medicamentSearchText;
            set
            {
                _medicamentSearchText = value;
                OnPropertyChanged();
                LoadMedicamente();
            }
        }

        private string _furnizorSearchText = string.Empty;
        public string FurnizorSearchText
        {
            get => _furnizorSearchText;
            set
            {
                _furnizorSearchText = value;
                OnPropertyChanged();
                LoadFurnizori();
            }
        }

        private Medicament? _selectedMedicamentFilter;
        public Medicament? SelectedMedicamentFilter
        {
            get => _selectedMedicamentFilter;
            set
            {
                _selectedMedicamentFilter = value;
                OnPropertyChanged();
                LoadAprovizionari();
            }
        }

        private Furnizor? _selectedFurnizorFilter;
        public Furnizor? SelectedFurnizorFilter
        {
            get => _selectedFurnizorFilter;
            set
            {
                _selectedFurnizorFilter = value;
                OnPropertyChanged();
                LoadAprovizionari();
            }
        }

        private DateTime? _aprovizionareFromFilter;
        public DateTime? AprovizionareFromFilter
        {
            get => _aprovizionareFromFilter;
            set
            {
                _aprovizionareFromFilter = value;
                OnPropertyChanged();
                LoadAprovizionari();
            }
        }

        private DateTime? _aprovizionareToFilter;
        public DateTime? AprovizionareToFilter
        {
            get => _aprovizionareToFilter;
            set
            {
                _aprovizionareToFilter = value;
                OnPropertyChanged();
                LoadAprovizionari();
            }
        }

        public RelayCommand AddMedicamentCommand { get; }
        public RelayCommand EditMedicamentCommand { get; }
        public RelayCommand DeleteMedicamentCommand { get; }
        public RelayCommand AddFurnizorCommand { get; }
        public RelayCommand EditFurnizorCommand { get; }
        public RelayCommand DeleteFurnizorCommand { get; }
        public RelayCommand AddAprovizionareCommand { get; }
        public RelayCommand EditAprovizionareCommand { get; }
        public RelayCommand DeleteAprovizionareCommand { get; }
        public RelayCommand ResetAprovizionareFiltersCommand { get; }
        public RelayCommand OpenReportCommand { get; }

        public MainViewModel()
        {
            AddMedicamentCommand = new RelayCommand(AddMedicament);
            EditMedicamentCommand = new RelayCommand(EditMedicament, () => SelectedMedicament != null);
            DeleteMedicamentCommand = new RelayCommand(DeleteMedicament, () => SelectedMedicament != null);

            AddFurnizorCommand = new RelayCommand(AddFurnizor);
            EditFurnizorCommand = new RelayCommand(EditFurnizor, () => SelectedFurnizor != null);
            DeleteFurnizorCommand = new RelayCommand(DeleteFurnizor, () => SelectedFurnizor != null);

            AddAprovizionareCommand = new RelayCommand(AddAprovizionare);
            EditAprovizionareCommand = new RelayCommand(EditAprovizionare, () => SelectedAprovizionare != null);
            DeleteAprovizionareCommand = new RelayCommand(DeleteAprovizionare, () => SelectedAprovizionare != null);
            ResetAprovizionareFiltersCommand = new RelayCommand(ResetAprovizionareFilters);
            OpenReportCommand = new RelayCommand(OpenReport);

            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadMedicamenteFilters();
            LoadFurnizoriFilters();
            LoadMedicamente();
            LoadFurnizori();
            LoadAprovizionari();
        }

        private void LoadMedicamenteFilters()
        {
            var selectedId = SelectedMedicamentFilter?.IdMedicament ?? 0;
            MedicamenteForFilter.Clear();
            MedicamenteForFilter.Add(new Medicament { IdMedicament = 0, Denumire = "Toate medicamentele" });
            foreach (var item in _db.GetMedicamente())
            {
                MedicamenteForFilter.Add(item);
            }

            SelectedMedicamentFilter = MedicamenteForFilter.FirstOrDefault(x => x.IdMedicament == selectedId) ?? MedicamenteForFilter[0];
        }

        private void LoadFurnizoriFilters()
        {
            var selectedId = SelectedFurnizorFilter?.IdFurnizor ?? 0;
            FurnizoriForFilter.Clear();
            FurnizoriForFilter.Add(new Furnizor { IdFurnizor = 0, Denumire = "Toti furnizorii" });
            foreach (var item in _db.GetFurnizori())
            {
                FurnizoriForFilter.Add(item);
            }

            SelectedFurnizorFilter = FurnizoriForFilter.FirstOrDefault(x => x.IdFurnizor == selectedId) ?? FurnizoriForFilter[0];
        }

        private void LoadMedicamente()
        {
            Medicamente.Clear();
            foreach (var item in _db.GetMedicamente(MedicamentSearchText))
            {
                Medicamente.Add(item);
            }
        }

        private void LoadFurnizori()
        {
            Furnizori.Clear();
            foreach (var item in _db.GetFurnizori(FurnizorSearchText))
            {
                Furnizori.Add(item);
            }
        }

        private void LoadAprovizionari()
        {
            Aprovizionari.Clear();
            var medicamentId = SelectedMedicamentFilter?.IdMedicament ?? 0;
            var furnizorId = SelectedFurnizorFilter?.IdFurnizor ?? 0;
            foreach (var item in _db.GetAprovizionari(medicamentId, furnizorId, AprovizionareFromFilter, AprovizionareToFilter))
            {
                Aprovizionari.Add(item);
            }
        }

        private void AddMedicament()
        {
            var dialog = new MedicamentDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void EditMedicament()
        {
            if (SelectedMedicament == null)
            {
                return;
            }

            var dialog = new MedicamentDialog(SelectedMedicament);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void DeleteMedicament()
        {
            if (SelectedMedicament == null)
            {
                return;
            }

            if (MessageBox.Show(
                    $"Удалить лекарство \"{SelectedMedicament.Denumire}\"?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            _db.DeleteMedicament(SelectedMedicament.IdMedicament);
            LoadAllData();
        }

        private void AddFurnizor()
        {
            var dialog = new FurnizorDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void EditFurnizor()
        {
            if (SelectedFurnizor == null)
            {
                return;
            }

            var dialog = new FurnizorDialog(SelectedFurnizor);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void DeleteFurnizor()
        {
            if (SelectedFurnizor == null)
            {
                return;
            }

            if (MessageBox.Show(
                    $"Удалить поставщика \"{SelectedFurnizor.Denumire}\" и связанные поставки?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _db.DeleteFurnizor(SelectedFurnizor.IdFurnizor);
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAprovizionare()
        {
            var dialog = new AprovizionareDialog(MedicamenteForFilter, FurnizoriForFilter);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void EditAprovizionare()
        {
            if (SelectedAprovizionare == null)
            {
                return;
            }

            var dialog = new AprovizionareDialog(MedicamenteForFilter, FurnizoriForFilter, SelectedAprovizionare);
            if (dialog.ShowDialog() == true)
            {
                LoadAllData();
            }
        }

        private void DeleteAprovizionare()
        {
            if (SelectedAprovizionare == null)
            {
                return;
            }

            if (MessageBox.Show(
                    "Удалить выбранную поставку?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _db.DeleteAprovizionare(SelectedAprovizionare.IdAprovizionare);
                LoadAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetAprovizionareFilters()
        {
            AprovizionareFromFilter = null;
            AprovizionareToFilter = null;
            SelectedMedicamentFilter = MedicamenteForFilter.Count > 0 ? MedicamenteForFilter[0] : null;
            SelectedFurnizorFilter = FurnizoriForFilter.Count > 0 ? FurnizoriForFilter[0] : null;
            LoadAprovizionari();
        }

        private void OpenReport()
        {
            var window = new ReportWindow(_db);
            window.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
