using System;

namespace CourseCenterWPF.Models
{
    public class Aprovizionare
    {
        public int IdAprovizionare { get; set; }
        public int IdMedicament { get; set; }
        public int IdFurnizor { get; set; }
        public DateTime DataAprovizionare { get; set; }
        public int Cantitate { get; set; }
        public decimal PretAchizitie { get; set; }
        public DateTime DataExpirare { get; set; }

        public string MedicamentDenumire { get; set; } = string.Empty;
        public string FurnizorDenumire { get; set; } = string.Empty;
        public decimal CostTotal => decimal.Round(Cantitate * PretAchizitie, 2);
    }
}
