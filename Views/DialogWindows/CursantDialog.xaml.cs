using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using CourseCenterWPF.Models;
using CourseCenterWPF.Services;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class CursantDialog : Window
    {
        private Cursant _cursant;
        private DatabaseService _db = new DatabaseService();
        private bool _isEdit;

        public CursantDialog(Cursant cursant = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            if (cursant != null)
            {
                _cursant = cursant;
                _isEdit = true;
                txtNume.Text = cursant.Nume;
                txtPrenume.Text = cursant.Prenume;
                txtTelefon.Text = cursant.Telefon;
                txtEmail.Text = cursant.Email;
                Title = "✏️ Редактирование слушателя";
            }
            else
            {
                _cursant = new Cursant();
                _isEdit = false;
                Title = "➕ Новый слушатель";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNume.Text) ||
                    string.IsNullOrWhiteSpace(txtPrenume.Text) ||
                    string.IsNullOrWhiteSpace(txtTelefon.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Все поля обязательны для заполнения!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Regex.IsMatch(txtTelefon.Text, @"^\+373\d{8}$"))
                {
                    MessageBox.Show("Телефон должен быть в формате: +373XXXXXXXX", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string checkEmail = $"SELECT COUNT(*) FROM Cursant WHERE Email = '{txtEmail.Text}'";
                if (_isEdit)
                    checkEmail += $" AND IdCursant != {_cursant.IdCursant}";

                DataTable dt = _db.ExecuteQuery(checkEmail);
                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Слушатель с таким email уже существует!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string query;
                if (_isEdit)
                {
                    query = $"UPDATE Cursant SET Nume='{txtNume.Text}', Prenume='{txtPrenume.Text}', " +
                            $"Telefon='{txtTelefon.Text}', Email='{txtEmail.Text}' " +
                            $"WHERE IdCursant={_cursant.IdCursant}";
                }
                else
                {
                    query = $"INSERT INTO Cursant (Nume, Prenume, Telefon, Email) VALUES " +
                            $"('{txtNume.Text}', '{txtPrenume.Text}', '{txtTelefon.Text}', '{txtEmail.Text}')";
                }

                _db.ExecuteNonQuery(query);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}