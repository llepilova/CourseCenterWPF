using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class AprovizionareDialog : Window
    {
        private readonly DatabaseService _db = new();
        private readonly Aprovizionare? _aprovizionare;
        private readonly bool _isEdit;
        private readonly ObservableCollection<Medicament> _medicamente;
        private readonly ObservableCollection<Furnizor> _furnizori;

        public AprovizionareDialog(ObservableCollection<Medicament> medicamente, ObservableCollection<Furnizor> furnizori, Aprovizionare? aprovizionare = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _medicamente = new ObservableCollection<Medicament>(medicamente.Where(x => x.IdMedicament > 0));
            _furnizori = new ObservableCollection<Furnizor>(furnizori.Where(x => x.IdFurnizor > 0));
            _aprovizionare = aprovizionare;
            _isEdit = aprovizionare != null;

            cmbMedicament.ItemsSource = _medicamente;
            cmbFurnizor.ItemsSource = _furnizori;
            dpDataAprovizionare.SelectedDate = DateTime.Today;
            dpDataExpirare.SelectedDate = DateTime.Today.AddYears(1);
            txtCantitate.Text = "1";
            txtPretAchizitie.Text = "1";

            if (_isEdit && _aprovizionare != null)
            {
                cmbMedicament.SelectedValue = _aprovizionare.IdMedicament;
                cmbFurnizor.SelectedValue = _aprovizionare.IdFurnizor;
                dpDataAprovizionare.SelectedDate = _aprovizionare.DataAprovizionare;
                txtCantitate.Text = _aprovizionare.Cantitate.ToString();
                txtPretAchizitie.Text = _aprovizionare.PretAchizitie.ToString(CultureInfo.InvariantCulture);
                dpDataExpirare.SelectedDate = _aprovizionare.DataExpirare;
            }
            else
            {
                if (_medicamente.Count > 0)
                {
                    cmbMedicament.SelectedIndex = 0;
                }

                if (_furnizori.Count > 0)
                {
                    cmbFurnizor.SelectedIndex = 0;
                }
            }

            RefreshCalculatedFields();
        }

        private void InputChanged(object sender, EventArgs e) => RefreshCalculatedFields();

        private void RefreshCalculatedFields()
        {
            if (cmbMedicament.SelectedItem is not Medicament || cmbFurnizor.SelectedItem is not Furnizor)
            {
                txtCostTotal.Text = "0.00";
                return;
            }

            if (!TryParseDecimal(txtPretAchizitie.Text, out var price))
            {
                price = 0;
            }

            if (!int.TryParse(txtCantitate.Text, out var qty))
            {
                qty = 0;
            }

            txtCostTotal.Text = Math.Round(price * qty, 2).ToString("F2", CultureInfo.InvariantCulture);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMedicament.SelectedItem is not Medicament medicament)
            {
                MessageBox.Show("Выберите лекарство.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbFurnizor.SelectedItem is not Furnizor furnizor)
            {
                MessageBox.Show("Выберите поставщика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpDataAprovizionare.SelectedDate.HasValue || !dpDataExpirare.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите даты поставки и истечения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantitate.Text, out var cantitate) || cantitate <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseDecimal(txtPretAchizitie.Text, out var pretAchizitie) || pretAchizitie <= 0)
            {
                MessageBox.Show("Закупочная цена должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDataExpirare.SelectedDate.Value.Date <= dpDataAprovizionare.SelectedDate.Value.Date)
            {
                MessageBox.Show("Дата истечения должна быть позже даты поставки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var model = _aprovizionare ?? new Aprovizionare();
                model.IdMedicament = medicament.IdMedicament;
                model.IdFurnizor = furnizor.IdFurnizor;
                model.DataAprovizionare = dpDataAprovizionare.SelectedDate.Value;
                model.Cantitate = cantitate;
                model.PretAchizitie = pretAchizitie;
                model.DataExpirare = dpDataExpirare.SelectedDate.Value;

                _db.SaveAprovizionare(model, _isEdit);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения поставки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
                   decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
