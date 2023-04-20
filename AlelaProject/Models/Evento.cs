using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlelaProject.Models
{
    public class Evento
    {
        public string Descripcion { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsFullDay { get; set; }
        public string Subject { get; set; }
        public string ThemeColor { get; set; }

        public string NombreCompleto { get; set; }

    }
}