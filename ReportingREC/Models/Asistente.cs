using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportingREC.Models
{
    class Asistente
    {
        public string emp_code { get; set; }
        public string first_name { get; set; }
        public DateTime punch_time { get; set; }
        public string passport { get; set; }
        public string Asistio { get; set; }
        public string Reservo { get; set; }

        public string ReservoNoAsistio { get; set; }
    }
}
