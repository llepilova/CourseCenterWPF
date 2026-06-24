using CourseCenterWPF.Models;
using CourseCenterWPF.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace CourseCenterWPF.Views.DialogWindows
{
    public partial class FurnizorDialog : Window
    {
        private readonly DatabaseService _db = new();
        private readonly Furnizor _furnizor;
        private readonly bool _isEdit;

        public FurnizorDialog(Furnizor? furnizor = null)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _isEdit = furnizor != null;
            _furnizor = furnizor ?? new Furnizor();

            txtDenumire.Text = _furnizor.Denumire;
            txtTelefon.Text = _furnizor.Telefon;
            txtEmail.Text = _furnizor.Email;
            txtAdresa.Text = _furnizor.Adresa;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDenumire.Text) ||
                string.IsNullOrWhiteSpace(txtTelefon.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtAdresa.Text))
            {
                MessageBox.Show("Все обязательные поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!new EmailAddressAttribute().IsValid(txtEmail.Text.Trim()))
            {
                MessageBox.Show("Email имеет неверный формат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _furnizor.Denumire = txtDenumire.Text.Trim();
                _furnizor.Telefon = txtTelefon.Text.Trim();
                _furnizor.Email = txtEmail.Text.Trim();
                _furnizor.Adresa = txtAdresa.Text.Trim();

                _db.SaveFurnizor(_furnizor, _isEdit);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения поставщика: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
