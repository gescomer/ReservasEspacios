using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class TableroControlController : Controller
    {
        public static string idUsuario { get; set; }
        public static string CorreoElectronico { get; set; }
        public static string idPerfil { get; set; }
        public static List<Datos> _datosgrafico = new List<Datos>();

        string conexionData = Helper.Coneccion;
        // GET: TableroControl
        public ActionResult Index()
        {
            ViewBag.NombreCompleto = System.Web.HttpContext.Current.Session["NombreCompleto"] as String;
            ViewBag.IdUser = System.Web.HttpContext.Current.Session["IdUser"] as String;
            CorreoElectronico = System.Web.HttpContext.Current.Session["Correo"] as String;
            ViewBag.idPerfil = System.Web.HttpContext.Current.Session["idPerfil"] as String;
            ViewBag.NombrePerfil = System.Web.HttpContext.Current.Session["NombrePerfil"] as String;
            ViewBag.urlima = System.Web.HttpContext.Current.Session["urlima"] as String;


            if (string.IsNullOrEmpty(ViewBag.NombreCompleto))
            {
                return RedirectToAction("Index", "Home");
            }
            var Fechahoy = DateTime.Now.ToString("yyyy-MM-dd");

            string nombredia = DateTime.Now.ToString("dddd");

            ViewBag.FechaConsulta = Fechahoy;

            DateTime Fechainicial, FechaFinal, fh;

            fh = System.DateTime.Now;
            switch (nombredia)
            {
                case "lunes":
                    Fechainicial = fh;
                    FechaFinal = fh.AddDays(5);
                    IndiceRiesgo(Fechainicial, FechaFinal);
                    break;
                case "martes":
                    Fechainicial = fh.AddDays(-1);
                    FechaFinal = fh.AddDays(4);
                    IndiceRiesgo(Fechainicial, FechaFinal);
                    break;
                case "miércoles":
                    Fechainicial = fh.AddDays(-2);
                    FechaFinal = fh.AddDays(2);
                    IndiceRiesgo(Fechainicial, FechaFinal);
                    break;
                case "jueves":
                    Fechainicial = fh.AddDays(-3);
                    FechaFinal = fh.AddDays(1);
                    IndiceRiesgo(Fechainicial, FechaFinal);
                    break;
                default:
                    Fechainicial = fh.AddDays(-4);
                    FechaFinal = fh;
                    IndiceRiesgo(Fechainicial, FechaFinal);
                    break;
            }
            var Consulta = "Select COUNT(tp.idUsuario) as Estadistica from DT_ControlUsuarios.dbo.tblusuarios tu inner join DT_ControlUsuarios.dbo.tblPermisos tp on tu.idUsuario = tp.idUsuario where idPortal = 4 and idEstado = 1 union all select count(*) from tblObjetos where idObjeto != 1224 union all select count(*) from tblReservas where Fecha = '" + Fechahoy + "' and idObjeto != 1224";
            int totalp = 0; int reservas = 0; int disponibilidad;
            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Datos>(Consulta).ToList();

                for (int i = 0; i < Data.Count; i++)
                {
                    if (i == 0)
                    {
                        ViewBag.Usuarios = Convert.ToInt32(Data[0].Estadistica);
                    }
                    if (i == 1)
                    {
                        ViewBag.Objetos = Convert.ToInt32(Data[1].Estadistica);
                        totalp = Convert.ToInt32(Data[1].Estadistica);
                    }
                    if (i == 2)
                    {
                        ViewBag.Reservas = Convert.ToInt32(Data[2].Estadistica);
                        reservas = Convert.ToInt32(Data[2].Estadistica);
                    }
                }
            }

            disponibilidad = totalp - reservas;
            ViewBag.Disponibles = disponibilidad;

            //Consultamos la ocupacion para el dia actual
            List<MiReservas> reservashoy = new List<MiReservas>();

            Consulta = "select NombreCompleto,NOMBRE,HoraInicio,HoraFin from tblReservas tr " +
                "inner join tblObjetos tob on tr.idObjeto = tob.idObjeto " +
                "inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario " +
                "where Fecha = '" + fh.ToString("yyyy-MM-dd") + "' and tr.idObjeto != 1224";

            using (var conexion = new SqlConnection(conexionData))
            {
                var _reservashoy = conexion.Query<MiReservas>(Consulta).ToList();
                if (_reservashoy.Count == 0)
                {
                    ViewBag.reservashoy = reservashoy;
                }
                else
                {
                    ViewBag.reservashoy = _reservashoy;
                }
            }

            return View();
        }

        public List<Datos> IndiceRiesgo(DateTime f1, DateTime f2)
        {
            string Consulta;
            _datosgrafico.Clear();

            Consulta = "Select Fecha from tblReservas where idObjeto != 1224 and Fecha between '" + f1.ToString("yyyy-MM-dd") + "' AND '" + f2.ToString("yyyy-MM-dd") + "' ORDER BY Fecha ASC";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<MiReservas>(Consulta).ToList();

                while (f1 <= f2)
                {
                    int contador = 0;
                    for (int o = 0; o < Data.Count; o++)
                    {
                        if (f1.ToString("yyyy-MM-dd") == Data[o].Fecha.ToString("yyyy-MM-dd"))
                        {
                            contador += 1;
                        }
                    }
                    string nombredia = f1.ToString("dddd");


                    _datosgrafico.Add(new Datos
                    {
                        NombreDia = nombredia,
                        Numero = contador,
                        Estadistica = 0
                    });
                    f1 += new TimeSpan(1, 0, 0, 0);
                }


            }

            return _datosgrafico;

        }
        public List<Datos> IndiceRiesgoGrafico()
        {
            return _datosgrafico;
        }
        public ActionResult VisualizacionIndice()
        {
            return Json(IndiceRiesgoGrafico(), JsonRequestBehavior.AllowGet);
        }

        public List<Datos> MasReservas()
        {
            string Consulta;
            List<Datos> datos = new List<Datos>();
            string Fecha = System.DateTime.Now.ToString("yyyy-MM-dd");
            Consulta = "select top 5 COUNT(idReserva) as reservas,NombreCompleto " +
                "from tblReservas tr inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario " +
                "where Fecha <= '" + Fecha + "' and idObjeto != 1224 and tr.idObjeto != 1221 group by NombreCompleto order by reservas desc";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Datos>(Consulta).ToList();
                datos = Data;
            }

            
            return datos;
        }
        public ActionResult VisualizacionMasReservas()
        {
            return Json(MasReservas(), JsonRequestBehavior.AllowGet);
        }
    }
}