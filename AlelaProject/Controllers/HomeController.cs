using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AlelaProject.Controllers
{
    public class HomeController : Controller
    {

        public static string NombreCompleto { get; set; }
        public ActionResult Index(string mensaje)
        {

            if (Request.Cookies["DATOSLOGN"] == null)
            {
                ViewBag.Error = mensaje; ;
                return View();

            }
            else
            {
                HttpCookie Cok = Request.Cookies["DATOSLOGN"];
                string idUser = Request.Cookies["DATOSLOGN"].Value; ;

                string conexionData = Helper.ConeccionUser;

                var Consulta = "Select tpm.idUsuario,NombreCompleto,tpm.idPerfil,NombrePerfil,Correo from tblpermisos tpm inner join tblPortales tpt on tpm.idPortal = tpt.idPortal inner join tblUsuarios tu on tpm.idUsuario = tu.idUsuario inner join tblPerfiles tp on tpm.idPerfil = tp.idPerfil where tu.idUsuario = " + idUser + " and tpm.idportal = 4";

                ViewBag.Error = idUser;

                using (var conexion = new SqlConnection(conexionData))
                {
                    var Userlogg = conexion.Query<Usuario>(Consulta).ToList();

                    if (Userlogg.Count == 0)
                    {
                        ViewBag.Error = "Usuario o Contraseña Invalidos";
                        return View();
                    }
                    else
                    {
                        foreach (var item in Userlogg)
                        {
                            System.Web.HttpContext.Current.Session["NombreCompleto"] = item.NombreCompleto.ToUpper();
                            System.Web.HttpContext.Current.Session["IdUser"] = item.idUsuario;
                            System.Web.HttpContext.Current.Session["Correo"] = item.correo;
                            System.Web.HttpContext.Current.Session["idPerfil"] = item.idPerfil;
                            System.Web.HttpContext.Current.Session["NombrePerfil"] = item.NombrePerfil;
                        }
                        return RedirectToAction("Index", "MapaCompleto");
                    }
                }

            }

        }

        [HttpPost]
        public ActionResult IndexPost(string Beta, string Cornelio)
        {
            if (string.IsNullOrEmpty(Beta) || string.IsNullOrEmpty(Cornelio))
            {
                Helper.Mensaje = "No se pudo ingresar el centro de costo. Verifique la informacion.";
                Helper.Controller = "/Home/Index/";
                return RedirectToAction("Index", "MensajeGenerico");
            }
            try
            {
                Beta = Helper.TipoOracion(Beta);

                string _Pass = Helper.Encryptation(Cornelio);

                string conexionData = Helper.ConeccionUser;
                var Consulta = "select tp.idUsuario,tp.idPerfil, NombreCompleto,Usuario as UsuarioRed,Correo,NombrePerfil,URL as uril  from tblPermisos tp inner join tblUsuarios tu on tp.idUsuario = tu.idUsuario inner join tblPerfiles tpf on tp.idPerfil = tpf.idPerfil where tu.Usuario = '" + Beta + "' and Contrasena = '" + _Pass + "' and idPortal = 4 and idEstado = 1";

                using (var conexion = new SqlConnection(conexionData))
                {
                    var Userlogg = conexion.Query<Usuario>(Consulta).ToList();
                    if (Userlogg.Count == 0)
                    {

                        return RedirectToAction("Index", "Home", new { mensaje = "Usuario o contraseña no valido" });
                    }
                    else
                    {
                        foreach (var item in Userlogg)
                        {
                            System.Web.HttpContext.Current.Session["NombreCompleto"] = item.NombreCompleto;
                            System.Web.HttpContext.Current.Session["IdUser"] = item.idUsuario;
                            System.Web.HttpContext.Current.Session["Correo"] = item.correo;
                            System.Web.HttpContext.Current.Session["idPerfil"] = item.idPerfil;
                            System.Web.HttpContext.Current.Session["NombrePerfil"] = item.NombrePerfil;
                            if (string.IsNullOrEmpty(item.uril))
                            {
                                System.Web.HttpContext.Current.Session["urlima"] = "https://intranet.conhydra.com/wp-content/uploads/2020/08/logo-conhydra-web.png";
                            }
                            else
                            {
                                System.Web.HttpContext.Current.Session["urlima"] = item.uril;
                            }

                        }
                        return RedirectToAction("Index", "MapaCompleto");
                    }
                }
            }
            catch
            {
                Helper.Mensaje = "No se pudo ingresar al sistema. Verifique la informacion.";
                Helper.Controller = "/Home/Index/";
                System.Web.HttpContext.Current.Session["NombreCompleto"] = string.Empty;
                return RedirectToAction("Index", "MensajeGenerico");
            }
        }
        public ActionResult LoggOff()
        {
            //Limpiamos todas las variables
            System.Web.HttpContext.Current.Session["NombreCompleto"] = string.Empty;
            Session.Remove("NombreCompleto");
            try
            {
                HttpContext.User = new GenericPrincipal(new GenericIdentity(""), null);
                FormsAuthentication.SignOut();
                HttpCookie datosCok = Request.Cookies["DATOSLOGN"];
                datosCok.Expires = DateTime.Now.AddYears(-1);
                Response.Cookies.Add(datosCok);
            }
            catch
            {


            }
            return RedirectToAction("Index", "Home");
        }
    }
}