using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Windows;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class FilmDialog : Window
    {
        private readonly DatabaseService _db = new();
        private readonly Film _film;
        private readonly bool _isEdit;

        public FilmDialog(Film? film = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _isEdit = film != null;
            _film = film ?? new Film();

            if (_isEdit)
            {
                txtTitlu.Text = _film.Titlu;
                txtGen.Text = _film.Gen;
                txtDurata.Text = _film.DurataMinute.ToString();
                txtLimitaVarsta.Text = _film.LimitaVarsta.ToString();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitlu.Text) || string.IsNullOrWhiteSpace(txtGen.Text))
            {
                MessageBox.Show("Название и жанр обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDurata.Text, out var durata) || durata <= 0)
            {
                MessageBox.Show("Длительность должна быть больше 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtLimitaVarsta.Text, out var limitaVarsta) || limitaVarsta < 0)
            {
                MessageBox.Show("Возрастное ограничение должно быть >= 0.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEdit)
                {
                    _db.ExecuteNonQuery(
                        "UPDATE Filme SET Titlu = @titlu, Gen = @gen, DurataMinute = @durata, LimitaVarsta = @limita WHERE IdFilm = @id",
                        new SqliteParameter("@titlu", txtTitlu.Text.Trim()),
                        new SqliteParameter("@gen", txtGen.Text.Trim()),
                        new SqliteParameter("@durata", durata),
                        new SqliteParameter("@limita", limitaVarsta),
                        new SqliteParameter("@id", _film.IdFilm));
                }
                else
                {
                    _db.ExecuteNonQuery(
                        "INSERT INTO Filme (Titlu, Gen, DurataMinute, LimitaVarsta) VALUES (@titlu, @gen, @durata, @limita)",
                        new SqliteParameter("@titlu", txtTitlu.Text.Trim()),
                        new SqliteParameter("@gen", txtGen.Text.Trim()),
                        new SqliteParameter("@durata", durata),
                        new SqliteParameter("@limita", limitaVarsta));
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения фильма: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
