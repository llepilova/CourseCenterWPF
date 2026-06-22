using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using CourseCenterWPF.Models;
using CourseCenterWPF.Services;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class InscriereDialog : Window
    {
        private DatabaseService _db = new DatabaseService();
        private ObservableCollection<Cursant> _cursanti;
        private ObservableCollection<Curs> _cursuri;

        public InscriereDialog(ObservableCollection<Cursant> cursanti, ObservableCollection<Curs> cursuri)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _cursanti = cursanti;
            _cursuri = cursuri;

            cmbCursant.ItemsSource = _cursanti;
            cmbCurs.ItemsSource = _cursuri;
            cmbStatus.SelectedIndex = 0;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbCursant.SelectedItem == null || cmbCurs.SelectedItem == null)
                {
                    MessageBox.Show("Выберите слушателя и курс!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idCursant = (cmbCursant.SelectedItem as Cursant).IdCursant;
                int idCurs = (cmbCurs.SelectedItem as Curs).IdCurs;
                string status = (cmbStatus.SelectedItem as ComboBoxItem).Tag.ToString();

                string checkQuery = $"SELECT COUNT(*) FROM Inscriere WHERE IdCursant = {idCursant} AND IdCurs = {idCurs}";
                DataTable dt = _db.ExecuteQuery(checkQuery);
                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Слушатель уже зарегистрирован на этот курс!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string query = $"INSERT INTO Inscriere (IdCursant, IdCurs, DataInscriere, StatusPlata) VALUES " +
                               $"({idCursant}, {idCurs}, '{DateTime.Now:yyyy-MM-dd}', '{status}')";

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