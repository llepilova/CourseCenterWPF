using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseCenterWPF.Models
{
    public class Cursant
    {
        public int IdCursant { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }

        public string FullName => $"{Nume} {Prenume}";
    }
}