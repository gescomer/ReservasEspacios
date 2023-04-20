using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportingREC.Models
{
    class Reserva
    {
        public string idReserva { get; set; }
        public string NombreArea { get; set; }
        public string TipoObjeto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string FechaReserva { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public DateTime Fecha { get; set; }
        public string Eliminar { get; set; }
        public string NombreHora { get; set; }
        public string NombreCompleto { get; set; }
        public string Observaciones { get; set; }
        public string idUsuario { get; set; }
        public string Contrasena { get; set; }
        public string Asistio { get; set; }
        public string Correo { get; set; }
        public string Reservo { get; set; }
        public string Horallegada { get; set; }
        public string ReservoNoAsistio { get; set; }
    }
}
