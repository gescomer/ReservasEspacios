using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlelaProject.Models
{
    public class Asistente
    {
        public string emp_code { get; set; }
        public string first_name { get; set; }
        public DateTime punch_time { get; set; }
        public string passport { get; set; }
        public string Asistio { get; set; }
        public string Reservo { get; set; }
        public string emp_type_id { get; set; }
        public string fecha { get; set; }
        public string Llegada { get; set; }
        public string Salida { get; set; }
        public string variable { get; set; }
    }
}