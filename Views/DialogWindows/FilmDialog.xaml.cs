using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Globalization;
using System.Windows;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class MedicamentDialog : Window
    {
        private readonly DatabaseService _db = new();
        private readonly Medicament _medicament;
        private readonly bool _isEdit;

        public MedicamentDialog(Medicament? medicament = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _isEdit = medicament != null;
            _medicament = medicament ?? new Medicament();

            txtDenumire.Text = _medicament.Denumire;
            txtForma.Text = _medicament.FormaFarmaceutica;
            txtConcentratie.Text = _medicament.Concentratie;
            txtPret.Text = _medicament.Pret.ToString(CultureInfo.InvariantCulture);
            txtStoc.Text = _medicament.StocCurent.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDenumire.Text) ||
                string.IsNullOrWhiteSpace(txtForma.Text) ||
                string.IsNullOrWhiteSpace(txtConcentratie.Text))
            {
                MessageBox.Show("Все обязательные поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseDecimal(txtPret.Text, out var pret) || pret <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _medicament.Denumire = txtDenumire.Text.Trim();
                _medicament.FormaFarmaceutica = txtForma.Text.Trim();
                _medicament.Concentratie = txtConcentratie.Text.Trim();
                _medicament.Pret = pret;

                _db.SaveMedicament(_medicament, _isEdit);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения лекарства: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
