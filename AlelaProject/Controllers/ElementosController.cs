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
    public class ElementosController : Controller
    {
        // GET: Elementos
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
            var Consulta = "select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,NombreComplemento from tblObjetos tob inner join tblArea ta on tob.idArea = ta.idArea inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto left join tblObjetoComplemento toc on tob.idObjeto = toc.idObjeto left join tblComplemento tcm on toc.idComplemento = tcm.idComplemento";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Elemento>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    ViewBag.Error = "No hay Areas disponibles";
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