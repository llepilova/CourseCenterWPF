namespace CourseCenterWPF.Models
{
    public class RaportItem
    {
        public string MedicamentDenumire { get; set; } = string.Empty;
        public string FurnizorDenumire { get; set; } = string.Empty;
        public int NumarAprovizionari { get; set; }
        public int CantitateTotala { get; set; }
        public decimal CostTotal { get; set; }
        public int StocCurent { get; set; }
    }
}
