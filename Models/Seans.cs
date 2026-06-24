using System;

namespace CourseCenterWPF.Models
{
    public class Seans
    {
        public int IdSeansa { get; set; }
        public int IdFilm { get; set; }
        public DateTime DataSeansa { get; set; }
        public string OraSeansa { get; set; } = string.Empty;
        public decimal PretBilet { get; set; }
        public int NumarLocuriTotal { get; set; }

        public string FilmTitlu { get; set; } = string.Empty;
        public int TotalBileteVandute { get; set; }
        public int LocuriLibere => NumarLocuriTotal - TotalBileteVandute;
    }
}
