using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseCenterWPF.Models
{
    public class Curs
    {
        public int IdCurs { get; set; }
        public string Denumire { get; set; }
        public string Formator { get; set; }
        public decimal Pret { get; set; }
        public int DurataZile { get; set; }
    }
}