using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlelaProject.Models
{
    public class Mapa
    {
        public string idObjeto { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string NombreArea { get; set; }
        public string TipoObjeto { get; set; }
        public string idArea { get; set; }
        public string NombreComplemento { get; set; }
        public string ReservaActual { get; set; }
    }
}