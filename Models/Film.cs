namespace CourseCenterWPF.Models
{
    public class Film
    {
        public int IdFilm { get; set; }
        public string Titlu { get; set; } = string.Empty;
        public string Gen { get; set; } = string.Empty;
        public int DurataMinute { get; set; }
        public int LimitaVarsta { get; set; }
    }
}
