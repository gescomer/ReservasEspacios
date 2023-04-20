using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlelaProject.Models
{
    public class ConsultaReserva
    {
        public string idObjeto { get; set; }
        public DateTime Fecha { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public string NombreCompleto { get; set; }
        public string FechaReserva { get; set; }
        public string Disponible { get; set; }
        public string NombreHora { get; set; }
        public string idReserva { get; set; }
        public string Observaciones { get; set; }
        public string idUsuario { get; set; }
        public string NombreDia { get; set; }
    }
}