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
    public class SedesController : Controller
    {
        // GET: Sedes
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
                    ViewBag.Datos = Data;
                }
            }
            return View();
        }
    }
}