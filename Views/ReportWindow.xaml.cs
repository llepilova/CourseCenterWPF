using CourseCenterWPF.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace CourseCenterWPF.Views
{
    public partial class ReportWindow : Window
    {
        private readonly string _reportText;

        public ReportWindow(DatabaseService db)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            _reportText = db.BuildReportText();
            txtReport.Text = _reportText;
            txtSummary.Text = $"Отчет сформирован {DateTime.Now:dd.MM.yyyy HH:mm}.";
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Сохранить отчет",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, _reportText);
                MessageBox.Show("Отчет сохранен.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
