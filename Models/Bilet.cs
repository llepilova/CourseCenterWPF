using System;

namespace CourseCenterWPF.Models
{
    public class Bilet
    {
        public int IdBilet { get; set; }
        public int IdSeansa { get; set; }
        public decimal ReducereProcent { get; set; }
        public int NumarBilete { get; set; }
        public decimal SumaAchitata { get; set; }
        public DateTime DataVanzare { get; set; }

        public string FilmTitlu { get; set; } = string.Empty;
        public string SeansInfo { get; set; } = string.Empty;
    }
}
