using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class SeansDialog : Window
    {
        private readonly DatabaseService _db = new();
        private readonly Seans? _seans;
        private readonly bool _isEdit;

        public SeansDialog(ObservableCollection<Film> filme, Seans? seans = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _seans = seans;
            _isEdit = seans != null;
            cmbFilm.ItemsSource = filme;
            dpDataSeansa.SelectedDate = DateTime.Today;

            if (_isEdit && _seans != null)
            {
                cmbFilm.SelectedValue = _seans.IdFilm;
                dpDataSeansa.SelectedDate = _seans.DataSeansa;
                txtOraSeansa.Text = _seans.OraSeansa;
                txtPretBilet.Text = _seans.PretBilet.ToString(CultureInfo.InvariantCulture);
                txtNumarLocuriTotal.Text = _seans.NumarLocuriTotal.ToString();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbFilm.SelectedItem is not Film film)
            {
                MessageBox.Show("Выберите фильм.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpDataSeansa.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату сеанса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParseExact(txtOraSeansa.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show("Время должно быть в формате HH:mm.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPretBilet.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var pret) || pret <= 0)
            {
                MessageBox.Show("Цена билета должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtNumarLocuriTotal.Text, out var locuri) || locuri <= 0)
            {
                MessageBox.Show("Количество мест должно быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var data = dpDataSeansa.SelectedDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var ora = txtOraSeansa.Text.Trim();

            var duplicateQuery = "SELECT COUNT(*) FROM Seanse WHERE IdFilm = @idFilm AND DataSeansa = @data AND OraSeansa = @ora";
            var duplicateParams = new[]
            {
                new SqliteParameter("@idFilm", film.IdFilm),
                new SqliteParameter("@data", data),
                new SqliteParameter("@ora", ora)
            };

            if (_isEdit && _seans != null)
            {
                duplicateQuery += " AND IdSeansa <> @idSeansa";
                Array.Resize(ref duplicateParams, 4);
                duplicateParams[3] = new SqliteParameter("@idSeansa", _seans.IdSeansa);
            }

            var duplicateCount = Convert.ToInt32(_db.ExecuteScalar(duplicateQuery, duplicateParams) ?? 0);
            if (duplicateCount > 0)
            {
                MessageBox.Show("Этот фильм уже запланирован на ту же дату и время.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEdit && _seans != null)
            {
                var soldCount = Convert.ToInt32(_db.ExecuteScalar("SELECT COALESCE(SUM(NumarBilete),0) FROM Bilete WHERE IdSeansa = @id", new SqliteParameter("@id", _seans.IdSeansa)) ?? 0);
                if (locuri < soldCount)
                {
                    MessageBox.Show($"Нельзя уменьшить места ниже уже проданных ({soldCount}).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                if (_isEdit && _seans != null)
                {
                    _db.ExecuteNonQuery(
                        @"UPDATE Seanse
                          SET IdFilm = @idFilm, DataSeansa = @data, OraSeansa = @ora, PretBilet = @pret, NumarLocuriTotal = @locuri
                          WHERE IdSeansa = @id",
                        new SqliteParameter("@idFilm", film.IdFilm),
                        new SqliteParameter("@data", data),
                        new SqliteParameter("@ora", ora),
                        new SqliteParameter("@pret", pret),
                        new SqliteParameter("@locuri", locuri),
                        new SqliteParameter("@id", _seans.IdSeansa));
                }
                else
                {
                    _db.ExecuteNonQuery(
                        @"INSERT INTO Seanse (IdFilm, DataSeansa, OraSeansa, PretBilet, NumarLocuriTotal)
                          VALUES (@idFilm, @data, @ora, @pret, @locuri)",
                        new SqliteParameter("@idFilm", film.IdFilm),
                        new SqliteParameter("@data", data),
                        new SqliteParameter("@ora", ora),
                        new SqliteParameter("@pret", pret),
                        new SqliteParameter("@locuri", locuri));
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения сеанса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
