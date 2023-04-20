using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class MensajeGenericoERRORController : Controller
    {
        // GET: MensajeGenericoERROR
        public static string Aprobador { get; set; }
        public static string idPerfil;
        public static string idUsuario;
        public static string idProyecto;
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
            ViewBag.Error = string.Empty;
            return View();
        }
    }
}