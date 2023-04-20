using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class NuevoPuestoController : Controller
    {
        public string id { get; set; }
        public int idc { get; set; }
        string conexionData = Helper.Coneccion;
        // GET: NuevoPuesto
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
            ConsultaArea();
            ConsultaTipoObjeto();
            ConsultaComplementos();
            return View();
        }

        public void ConsultaArea()
        {

            var Consulta = "select idArea,NombreArea from tblArea";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Area>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    ViewBag.Data = null;
                }
                else
                {
                    List<SelectListItem> item = Data.ConvertAll(d =>
                    {
                        return new SelectListItem()
                        {
                            Text = d.NombreArea.ToString(),
                            Value = d.idarea.ToString(),
                            Selected = false
                        };
                    });

                    ViewBag.Data = item;
                }
            }
        }

        public void ConsultaTipoObjeto()
        {

            var Consulta = "select idTipoObjeto,TipoObjeto as Tipo from tblTipoObjeto";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data2 = conexion.Query<TipoObjeto>(Consulta).ToList();

                if (Data2.Count == 0)
                {
                    ViewBag.Data2 = null;
                }
                else
                {
                    List<SelectListItem> item = Data2.ConvertAll(d =>
                    {
                        return new SelectListItem()
                        {
                            Text = d.Tipo.ToString(),
                            Value = d.idTipoObjeto.ToString(),
                            Selected = false
                        };
                    });

                    ViewBag.Data2 = item;
                }
            }
        }

        public void ConsultaComplementos()
        {
            string conexionData = Helper.Coneccion;
            var Consulta = "select idComplemento,NombreComplemento from tblComplemento";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data3 = conexion.Query<Complemento>(Consulta).ToList();

                if (Data3.Count == 0)
                {
                    ViewBag.Error = "No hay Sedes disponibles";
                }
                else
                {
                    ViewBag.Datos3 = Data3;
                }
            }
        }

        public ActionResult IndexAceptarmuliple(string[] ids, string Area, string Nombre, string Codigo, string TipoObjeto)
        {
            //Insertamos el Elemento.
            var Consulta = "insert into tblObjetos(Nombre,Codigo,idArea,idTipoObjeto)values(@Nombre,@Codigo,@idArea,@idTipoObjeto)";

            using (var conexion = new SqlConnection(conexionData))
            {
                conexion.Execute(Consulta, new { Nombre = Nombre.ToUpper(), Codigo = Codigo, idArea = Area, idTipoObjeto = TipoObjeto });
            }

            //Consultamos el idgenerado

            var Consulta2 = "select idObjeto from tblObjetos where Nombre = '" + Nombre + "' and Codigo = '" + Codigo + "' and idArea = '" + Area + "'";
            string idObjeto;
            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Elemento>(Consulta2).ToList();

                if (Data.Count == 0)
                {
                    ViewBag.Error = "No hay Areas disponibles";
                }
                else
                {
                    ViewBag.Datos = Data;
                }
            }

            idObjeto = ViewBag.Datos[0].idObjeto;

            id = ids[0];

            string[] array = id.Split(',');

            idc = array[0].Count();

            if (idc > 0)
            {
                foreach (var item in array)
                {
                    try
                    {
                        Thread.Sleep(500);
                        //Insertamos el cada Complemento de cada objeto o puesto de trabajo.
                        var Consulta3 = "insert into tblObjetoComplemento(idObjeto,idComplemento)values(@idObjeto,@idComplemento)";

                        using (var conexion = new SqlConnection(conexionData))
                        {
                            conexion.Execute(Consulta3, new { idObjeto = idObjeto, idComplemento = item });
                        }
                    }
                    catch
                    {

                    }
                }

            }
            return RedirectToAction("Index", "Elementos");
        }

        public ActionResult GoToBack()
        {
            return RedirectToAction("Index", "Elementos");
        }
    }
}