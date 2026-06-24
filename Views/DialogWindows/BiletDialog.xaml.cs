using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class BiletDialog : Window
    {
        private readonly DatabaseService _db = new();
        private readonly Bilet? _bilet;
        private readonly bool _isEdit;
        private readonly ObservableCollection<Seans> _seanse;

        public BiletDialog(ObservableCollection<Seans> seanse, Bilet? bilet = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _seanse = new ObservableCollection<Seans>(seanse.Where(s => s.IdSeansa > 0));
            _bilet = bilet;
            _isEdit = bilet != null;

            cmbSeans.ItemsSource = _seanse;
            dpDataVanzare.SelectedDate = DateTime.Today;
            txtReducere.Text = "0";
            txtNumarBilete.Text = "1";

            if (_isEdit && _bilet != null)
            {
                cmbSeans.SelectedValue = _bilet.IdSeansa;
                dpDataVanzare.SelectedDate = _bilet.DataVanzare;
                txtReducere.Text = _bilet.ReducereProcent.ToString(CultureInfo.InvariantCulture);
                txtNumarBilete.Text = _bilet.NumarBilete.ToString();
            }
            else if (_seanse.Count > 0)
            {
                cmbSeans.SelectedIndex = 0;
            }

            RefreshCalculatedFields();
        }

        private void CmbSeans_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshCalculatedFields();
        private void InputChanged(object sender, TextChangedEventArgs e) => RefreshCalculatedFields();

        private void RefreshCalculatedFields()
        {
            var freeSeats = GetFreeSeats();
            txtLocuriLibere.Text = freeSeats >= 0 ? freeSeats.ToString() : "-";

            var price = GetTicketPrice();
            if (!decimal.TryParse(txtReducere.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var discount))
            {
                discount = 0;
            }

            if (!int.TryParse(txtNumarBilete.Text, out var count))
            {
                count = 0;
            }

            var sum = Math.Round(price * count * (1 - discount / 100m), 2);
            txtSuma.Text = $"{sum:F2}";
        }

        private decimal GetTicketPrice()
        {
            if (cmbSeans.SelectedItem is not Seans seans)
            {
                return 0;
            }

            var value = _db.ExecuteScalar("SELECT PretBilet FROM Seanse WHERE IdSeansa = @id", new SqliteParameter("@id", seans.IdSeansa));
            return value == null ? 0 : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        private int GetFreeSeats()
        {
            if (cmbSeans.SelectedItem is not Seans seans)
            {
                return -1;
            }

            var dt = _db.ExecuteQuery(
                @"SELECT NumarLocuriTotal,
                         COALESCE((SELECT SUM(NumarBilete) FROM Bilete WHERE IdSeansa = @idSeansa AND (@excludeId = 0 OR IdBilet <> @excludeId)), 0) AS Sold
                  FROM Seanse
                  WHERE IdSeansa = @idSeansa",
                new SqliteParameter("@idSeansa", seans.IdSeansa),
                new SqliteParameter("@excludeId", _isEdit && _bilet != null ? _bilet.IdBilet : 0));

            if (dt.Rows.Count == 0)
            {
                return -1;
            }

            var total = Convert.ToInt32(dt.Rows[0]["NumarLocuriTotal"]);
            var sold = Convert.ToInt32(dt.Rows[0]["Sold"]);
            return total - sold;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSeans.SelectedItem is not Seans seans)
            {
                MessageBox.Show("Выберите сеанс.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpDataVanzare.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату продажи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtReducere.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var reducere) ||
                reducere < 0 || reducere > 100)
            {
                MessageBox.Show("Скидка должна быть в диапазоне от 0 до 100.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtNumarBilete.Text, out var numarBilete) || numarBilete <= 0)
            {
                MessageBox.Show("Количество билетов должно быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var freeSeats = GetFreeSeats();
            if (numarBilete > freeSeats)
            {
                MessageBox.Show($"Недостаточно свободных мест. Доступно: {freeSeats}.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var price = GetTicketPrice();
            var sum = Math.Round(price * numarBilete * (1 - reducere / 100m), 2);
            var dataVanzare = dpDataVanzare.SelectedDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            try
            {
                if (_isEdit && _bilet != null)
                {
                    _db.ExecuteNonQuery(
                        @"UPDATE Bilete
                          SET IdSeansa = @idSeansa, ReducereProcent = @reducere, NumarBilete = @numar, SumaAchitata = @suma, DataVanzare = @data
                          WHERE IdBilet = @id",
                        new SqliteParameter("@idSeansa", seans.IdSeansa),
                        new SqliteParameter("@reducere", reducere),
                        new SqliteParameter("@numar", numarBilete),
                        new SqliteParameter("@suma", sum),
                        new SqliteParameter("@data", dataVanzare),
                        new SqliteParameter("@id", _bilet.IdBilet));
                }
                else
                {
                    _db.ExecuteNonQuery(
                        @"INSERT INTO Bilete (IdSeansa, ReducereProcent, NumarBilete, SumaAchitata, DataVanzare)
                          VALUES (@idSeansa, @reducere, @numar, @suma, @data)",
                        new SqliteParameter("@idSeansa", seans.IdSeansa),
                        new SqliteParameter("@reducere", reducere),
                        new SqliteParameter("@numar", numarBilete),
                        new SqliteParameter("@suma", sum),
                        new SqliteParameter("@data", dataVanzare));
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения продажи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
