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

    public class MisReservasController : Controller
    {
        // GET: MisReservas
        public ActionResult Index()
        {
            ViewBag.NombreCompleto = System.Web.HttpContext.Current.Session["NombreCompleto"] as String;
            ViewBag.IdUser = System.Web.HttpContext.Current.Session["IdUser"] as String;
            ViewBag.idPerfil = System.Web.HttpContext.Current.Session["idPerfil"] as String; ;
            ViewBag.NombrePerfil = System.Web.HttpContext.Current.Session["NombrePerfil"] as String;
            ViewBag.urlima = System.Web.HttpContext.Current.Session["urlima"] as String;


            if (string.IsNullOrEmpty(ViewBag.NombreCompleto))
            {
                return RedirectToAction("Index", "Home");
            }
            string conexionData = Helper.Coneccion;
            string idu = ViewBag.IdUser;

            var Consulta = "select idReserva, NombreArea,TipoObjeto,Codigo,Nombre,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,HoraInicio,HoraFin,NombreHora,Fecha from tblreservas tr inner join tblObjetos tob on tr.idObjeto = tob.idObjeto inner join tblArea ta on tob.idArea = ta.idArea inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto left join tblHoraAlmuerzo tha on tr.idHoraAlmuerzo = tha.idHora where idUsuario = " + idu + " order by idReserva desc";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<MiReservas>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    List<MiReservas> _miReservas = new List<MiReservas>();
                    ViewBag.Datos = _miReservas;
                }
                else
                {
                    foreach (var item in Data)
                    {
                        if (item.Fecha > System.DateTime.Now)
                        {
                            item.Eliminar = "No";
                        }
                        else if (item.Fecha.Date == System.DateTime.Now.Date)
                        {
                            item.Eliminar = "Hoy";
                        }
                        else
                        {
                            item.Eliminar = "Si";
                        }
                    }
                    ViewBag.Datos = Data;

                }
            }
            return View();
        }

        public async Task<ActionResult> EliminarReserva(string ids, string reserva,string NombrePuesto)
        {
            string conexionData = Helper.Coneccion;
            //Insertamos el Elemento.
            var Consulta = "delete from tblReservas where idReserva = @idReserva";

            using (var conexion = new SqlConnection(conexionData))
            {
                conexion.Execute(Consulta, new { idReserva = ids });
            }
            string Correo = System.Web.HttpContext.Current.Session["Correo"] as string;
            await Helper.EliminarConfirmacion(reserva, Correo,NombrePuesto);
            return RedirectToAction("Index", "MisReservas");
        }
    }
}