namespace CourseCenterWPF.Models
{
    public class Medicament
    {
        public int IdMedicament { get; set; }
        public string Denumire { get; set; } = string.Empty;
        public string FormaFarmaceutica { get; set; } = string.Empty;
        public string Concentratie { get; set; } = string.Empty;
        public decimal Pret { get; set; }
        public int StocCurent { get; set; }
    }
}
