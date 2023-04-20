using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{

    public class AlmuerzoController : Controller
    {
        public static List<HoraAlmuerzo> mishorasAluerzo = new List<HoraAlmuerzo>();
        public static string idUsuario { get; set; }
        public static string CorreoElectronico { get; set; }
        public static string idPerfil { get; set; }
        string conexionData = Helper.Coneccion;

        // GET: Almuerzo
        public ActionResult Index(string FechaBusqueda)
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

            if (string.IsNullOrEmpty(FechaBusqueda))
            {

                ViewBag.FechaConsulta = DateTime.Now.ToString("yyyy-MM-dd");

                ConsultaAlmuerzo(ViewBag.FechaConsulta);

            }
            else
            {
                ConsultaAlmuerzo(FechaBusqueda);
                ViewBag.FechaConsulta = FechaBusqueda;
            }
            ConsultaHoraAlmuerzo();
            return View();
        }

        public void ConsultaAlmuerzo(string Fecha)
        {
            string Consulta = "SELECT idReserva,NombreCompleto,NombreHora,CONVERT(varchar,Fecha,105) as FechaReserva FROM tblReservas tr inner join tblHoraAlmuerzo th on tr.idHoraAlmuerzo = th.idHora inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tu.idUsuario = tr.idUsuario where Fecha = '" + Fecha + "' and idHora != 1002 order by idHora asc";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<ConsultaReserva>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    List<ConsultaReserva> consultaReservas = new List<ConsultaReserva>();
                    ViewBag.Datos = consultaReservas;
                }
                else
                {
                    ViewBag.Datos = Data;
                }

            }
        }

        public void ConsultaHoraAlmuerzo()
        {
            var Consulta = "select idHora,NombreHora from tblhoraalmuerzo";
            var Consulta2 = "select Fecha as FechaR,idHoraAlmuerzo from tblReservas";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<HoraAlmuerzo>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    List<HoraAlmuerzo> _miReservas = new List<HoraAlmuerzo>();
                    ViewBag.Almuerzo = _miReservas;
                }
                else
                {
                    List<SelectListItem> item = Data.ConvertAll(d =>
                    {
                        return new SelectListItem()
                        {
                            Text = d.NombreHora.ToString(),
                            Value = d.idHora.ToString(),
                            Selected = false
                        };
                    });

                    ViewBag.Almuerzo = item;

                }

                Data = conexion.Query<HoraAlmuerzo>(Consulta2).ToList();

                foreach (var item in Data)
                {
                    item.Fecha = (item.FechaR).ToString("yyyy-MM-dd");
                }

                ViewBag.almuerzoreservado = Data;
                mishorasAluerzo = Data;
            }

        }

        public async Task<ActionResult> IndexPost(string Fecha, string almuerzo, string HoraAlmuerzo)
        {
            string horainicio = "0";
            string horafin = "0";

            string usuario = System.Web.HttpContext.Current.Session["IdUser"] as String;

            //Verificamos que el usuario no tenga registrado la hora del almuerzo
            string Consulta2 = "select idHoraAlmuerzo from tblreservas where idusuario = " + usuario + " and fecha = '" + Fecha + "'";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<ConsultaReserva>(Consulta2).ToList();

                if (Data.Count == 0)
                {
                    //Si no tiene datos inserta la información
                    if (almuerzo == "1")
                    {
                        horainicio = "1200";
                        horafin = "1230";
                    }

                    if (almuerzo == "2")
                    {
                        horainicio = "1230";
                        horafin = "0100";
                    }

                    if (almuerzo == "3")
                    {
                        horainicio = "0100";
                        horafin = "0130";
                    }

                    if (almuerzo == "4")
                    {
                        horainicio = "0130";
                        horafin = "0200";
                    }

                    //Insertamos el Elemento.
                    var Consulta = "insert into tblReservas(idObjeto,Fecha,HoraInicio,HoraFin,idUsuario,idHoraAlmuerzo) values (@idObjeto,@Fecha,@HoraInicio,@HoraFin,@idUsuario,@idHoraAlmuerzo)";

                    using (var conexion2 = new SqlConnection(conexionData))
                    {
                        conexion2.Execute(Consulta, new
                        {
                            idObjeto = "1224",
                            Fecha = Fecha,
                            HoraInicio = horainicio,
                            HoraFin = horafin,
                            idUsuario = usuario,
                            idHoraAlmuerzo = almuerzo
                        });
                    }

                    await Helper.EnviarConfirmacionAlmuerzo(Fecha, HoraAlmuerzo, CorreoElectronico);

                    Helper.Mensaje = "Reserva de Almuerzo Ingresado Exitosamente ! ";

                    Helper.Controller = "/Almuerzo/Index/";

                    return RedirectToAction("Index", "MensajeGenericoOK");
                }
                else
                {
                    Helper.Mensaje = "Ya Tienes Registrada una hora de almuerzo. Verificar en el listado.";

                    Helper.Controller = "/Almuerzo/Index/";

                    return RedirectToAction("Index", "MensajeGenerico");
                }

            }
        }
    }
}