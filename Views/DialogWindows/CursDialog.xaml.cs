using System;
using System.Windows;
using CourseCenterWPF.Models;
using CourseCenterWPF.Services;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class CursDialog : Window
    {
        private Curs _curs;
        private DatabaseService _db = new DatabaseService();
        private bool _isEdit;

        public CursDialog(Curs curs = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            if (curs != null)
            {
                _curs = curs;
                _isEdit = true;
                txtDenumire.Text = curs.Denumire;
                txtFormator.Text = curs.Formator;
                txtPret.Text = curs.Pret.ToString();
                txtDurata.Text = curs.DurataZile.ToString();
                Title = "✏️ Редактирование курса";
            }
            else
            {
                _curs = new Curs();
                _isEdit = false;
                Title = "➕ Новый курс";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDenumire.Text) ||
                    string.IsNullOrWhiteSpace(txtFormator.Text) ||
                    string.IsNullOrWhiteSpace(txtPret.Text) ||
                    string.IsNullOrWhiteSpace(txtDurata.Text))
                {
                    MessageBox.Show("Все поля обязательны для заполнения!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPret.Text.Replace('.', ','), out decimal pret) || pret <= 0)
                {
                    MessageBox.Show("Стоимость должна быть больше 0!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtDurata.Text, out int durata) || durata <= 0)
                {
                    MessageBox.Show("Длительность должна быть положительным целым числом!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string query;
                if (_isEdit)
                {
                    query = $"UPDATE Curs SET Denumire='{txtDenumire.Text}', Formator='{txtFormator.Text}', " +
                            $"Pret={pret.ToString(System.Globalization.CultureInfo.InvariantCulture)}, DurataZile={durata} " +
                            $"WHERE IdCurs={_curs.IdCurs}";
                }
                else
                {
                    query = $"INSERT INTO Curs (Denumire, Formator, Pret, DurataZile) VALUES " +
                            $"('{txtDenumire.Text}', '{txtFormator.Text}', {pret.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {durata})";
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