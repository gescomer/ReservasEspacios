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
    public class UsuariosController : Controller
    {
        public static string CorreoElectronico { get; set; }
        // GET: Usuarios
        string conexionData = Helper.Coneccion;
        [OutputCache(Duration = 60)]
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

            var Consulta = "select NombreCompleto,Usuario as usuarioingreso,Correo,NombreEstado from DT_ControlUsuarios.dbo.tblPermisos tp inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tp.idUsuario = tu.idUsuario inner join DT_ControlUsuarios.dbo.tblEstados te on tp.idEstado = te.idEstado where idPortal = 4";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Usuario>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    List<Usuario> _usuarios = new List<Usuario>();
                    ViewBag.Datos = _usuarios;
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