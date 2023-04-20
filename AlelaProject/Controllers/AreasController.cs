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
    public class AreasController : Controller
    {
        // GET: Areas
        public string conexionData = Helper.Coneccion;
        [OutputCache(Duration = 600)]
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
            var Consulta = "select idarea,NombreArea,NombreSede from tblarea ta inner join tblSedes ts on ta.idsede = ts.idsede";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Area>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    ViewBag.Error = "No hay Areas disponibles";
                }
                else
                {
                    ViewBag.Datos = Data;
                }
            }
            ConsultaArea();
            return View();
        }

        public void ConsultaArea()
        {

            var Consulta = "select idSede,NombreSede from tblSedes";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Sede>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    ViewBag.Error = "No hay Sedes disponibles";
                }
                else
                {
                    List<SelectListItem> item = Data.ConvertAll(d =>
                    {
                        return new SelectListItem()
                        {
                            Text = d.NombreSede,
                            Value = d.idSede.ToString(),
                            Selected = false
                        };
                    });

                    ViewBag.Data = item;

                }
            }
        }

        public ActionResult IndexPost(string idSede, string nombrearea)
        {
            //Insertamos el Elemento.
            var Consulta = "insert into tblArea (NombreArea,idSede)values(@NombreArea,@idSede)";

            using (var conexion = new SqlConnection(conexionData))
            {
                conexion.Execute(Consulta, new { idSede = idSede, NombreArea = nombrearea });
            }

            Helper.Mensaje = "Reserva Ingresada Exitosamente ! ";
            Helper.Controller = "/MapaCompleto/Index/";
            return RedirectToAction("Index", "MensajeGenericoOK");
        }
    }
}