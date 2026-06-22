using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace CourseCenterWPF.Models
{
    public class Inscriere
    {
        public int IdInscriere { get; set; }
        public int IdCursant { get; set; }
        public int IdCurs { get; set; }
        public DateTime DataInscriere { get; set; }
        public string StatusPlata { get; set; }

        // Дополнительные поля для отображения
        public string NumeCursant { get; set; }
        public string DenumireCurs { get; set; }
    }
}