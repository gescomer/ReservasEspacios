using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class DemoController : Controller
    {
        string conexionData = Helper.Coneccion;
        private static List<ConsultaReserva> reservas = new List<ConsultaReserva>();
        public static string idUsuario { get; set; }
        // GET: Demo
        public ActionResult Index()
        {
            ViewBag.NombreCompleto = System.Web.HttpContext.Current.Session["NombreCompleto"] as String;
            ViewBag.IdUser = System.Web.HttpContext.Current.Session["IdUser"] as String;
            ViewBag.NombrePerfil = System.Web.HttpContext.Current.Session["NombrePerfil"] as String;
            idUsuario = ViewBag.IdUser;
            ViewBag.urlima = System.Web.HttpContext.Current.Session["urlima"] as String;
            if (string.IsNullOrEmpty(ViewBag.NombreCompleto))
            {
                return RedirectToAction("Index", "Home");
            }

            ConsultaPuestos();
            ConsultaHoras();
            ConsultaMapa();
            return View();

        }

        //Consultamos los puestos de trabajo para el modal
        public void ConsultaPuestos()
        {

            var Consulta = "select idObjeto,Nombre,Codigo,TipoObjeto  from tblObjetos tob inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Elemento>(Consulta).ToList();

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
                            Text = d.Nombre.ToString() + "  -  " + d.Codigo.ToString(),
                            Value = d.idObjeto.ToString(),
                            Selected = false
                        };
                    });

                    ViewBag.Data = item;
                }
            }
        }

        public void ConsultaHoras()
        {
            List<Hora> horas = new List<Hora>();
            horas.Add(new Hora { NumeroHora = "06:00", Value = "0600" });
            horas.Add(new Hora { NumeroHora = "06:30", Value = "0630" });
            horas.Add(new Hora { NumeroHora = "07:00", Value = "0700" });
            horas.Add(new Hora { NumeroHora = "07:30", Value = "0730" });
            horas.Add(new Hora { NumeroHora = "08:00", Value = "0800" });
            horas.Add(new Hora { NumeroHora = "08:30", Value = "0830" });
            horas.Add(new Hora { NumeroHora = "09:00", Value = "0900" });
            horas.Add(new Hora { NumeroHora = "09:30", Value = "0930" });
            horas.Add(new Hora { NumeroHora = "10:00", Value = "1000" });
            horas.Add(new Hora { NumeroHora = "10:30", Value = "1030" });
            horas.Add(new Hora { NumeroHora = "11:00", Value = "1100" });
            horas.Add(new Hora { NumeroHora = "11:30", Value = "1130" });
            horas.Add(new Hora { NumeroHora = "12:00", Value = "1200" });
            horas.Add(new Hora { NumeroHora = "12:30", Value = "1230" });
            horas.Add(new Hora { NumeroHora = "13:00", Value = "1300" });
            horas.Add(new Hora { NumeroHora = "13:30", Value = "1330" });
            horas.Add(new Hora { NumeroHora = "14:00", Value = "1400" });
            horas.Add(new Hora { NumeroHora = "14:30", Value = "1430" });
            horas.Add(new Hora { NumeroHora = "15:00", Value = "1500" });
            horas.Add(new Hora { NumeroHora = "15:30", Value = "1530" });
            horas.Add(new Hora { NumeroHora = "16:00", Value = "1600" });
            horas.Add(new Hora { NumeroHora = "16:30", Value = "1630" });
            horas.Add(new Hora { NumeroHora = "17:00", Value = "1700" });
            horas.Add(new Hora { NumeroHora = "17:30", Value = "1730" });
            horas.Add(new Hora { NumeroHora = "18:00", Value = "1800" });
            horas.Add(new Hora { NumeroHora = "18:30", Value = "1830" });

            List<SelectListItem> item = horas.ConvertAll(d =>
            {
                return new SelectListItem()
                {
                    Text = d.NumeroHora.ToString(),
                    Value = d.Value.ToString(),
                    Selected = false
                };
            });
            ViewBag.DataH = item;
            ViewBag.DataF = item;
        }

        public void ConsultaMapa()
        {
            string conexionData = Helper.Coneccion;
            var Consulta = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto";
            var ConsultaComple = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea,NombreComplemento  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto inner join tblObjetoComplemento toc on tob.idobjeto = toc.idObjeto left join tblComplemento tc on toc.idcomplemento = tc.idComplemento";
            var ConsultaReservas = "select idObjeto, Fecha,HoraInicio,HoraFin,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva from tblReservas tr inner join tblUsuario tu on tr.idUsuario = tu.idUsuario order by Fecha desc";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<Mapa>(Consulta).ToList();

                var DataComplementos = conexion.Query<Mapa>(ConsultaComple).ToList();

                //Seleccionamos los distintos IDObjeto
                var QP = from a in Data.Distinct()
                         select new { a.idObjeto };

                var result = QP.DistinctBy(i => i.idObjeto);

                List<ObjetosComplementos> objC = new List<ObjetosComplementos>();

                string Comple = string.Empty;

                foreach (var item in result)
                {

                    foreach (var objetos in DataComplementos)
                    {

                        if (item.idObjeto == objetos.idObjeto)
                        {
                            Comple = Comple + " " + objetos.NombreComplemento;
                        }
                    }

                    objC.Add(new ObjetosComplementos { idObjeto = item.idObjeto, Complementos = Comple });

                    Comple = string.Empty;
                }

                if (Data.Count == 0)
                {
                    ViewBag.Error = "Mapa no disponible";
                    ViewBag.DatosMapa = new List<Mapa>();
                    ViewBag.DatosMapaCompl = new List<ObjetosComplementos>();
                }
                else
                {
                    ViewBag.DatosMapa = Data;
                    ViewBag.DatosMapaCompl = objC;
                }

                //Buscamos las reservas de todos los puestos de trabajo
                var DataReser = conexion.Query<ConsultaReserva>(ConsultaReservas).ToList();
                if (DataReser.Count == 0)
                {
                    ViewBag.Error = "No hay Reservas disponibles";
                    ViewBag.DatosReservas = new List<ConsultaReserva>();
                }
                else
                {
                    reservas = DataReser;
                    foreach (var item in DataReser)
                    {
                        if (item.Fecha > System.DateTime.Now)
                        {
                            item.Disponible = "No Vencido";
                        }
                        else
                        {
                            item.Disponible = "Vencido";
                        }
                    }
                    ViewBag.DatosReservas = DataReser;
                }

                //Buscamos si cada puesto tiene reserva activa
                string fechahoy = DateTime.Today.ToString("yyyy-MM-dd");
                string complementos = string.Empty;
                foreach (var item in Data)
                {
                    #region SISTEMAS
                    if (item.idObjeto == "1209")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1209" == itemC.idObjeto)
                            {
                                ViewBag.PSistemasCom = complementos + " " + itemC.Complementos;

                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");

                            string horaactual = System.DateTime.Now.ToString("HH:mm");

                            if ("1209" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.ReservaActual = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.ReservaActual = "No";
                            }

                        }
                    }
                    if (item.idObjeto == "1210")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1210" == itemC.idObjeto)
                            {
                                ViewBag.PuestoOscar = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1210" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.ReservaActualOscar = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.ReservaActualOscar = "No";
                            }
                        }
                    }
                    #endregion

                    #region GESCOMER
                    if (item.idObjeto == "1190")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1190" == itemC.idObjeto)
                            {
                                ViewBag.gescomer1 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");

                            if (string.Equals(item2.idObjeto, "1190") && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservagescomer1 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1191")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1191" == itemC.idObjeto)
                            {
                                ViewBag.gescomer2 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1191" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservagescomer2 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1192")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1192" == itemC.idObjeto)
                            {
                                ViewBag.gescomer3 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1192" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservagescomer3 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1193")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1193" == itemC.idObjeto)
                            {
                                ViewBag.gescomer4 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1193" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer4 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservagescomer4 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1194")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1194" == itemC.idObjeto)
                            {
                                ViewBag.gescomer5 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1194" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer5 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservagescomer5 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1195")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1195" == itemC.idObjeto)
                            {
                                ViewBag.gescomer6 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1195" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer6 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservagescomer6 = "No";
                            }
                        }
                    }
                    #endregion

                    #region Comercial
                    if (item.idObjeto == "1196")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1190" == itemC.idObjeto)
                            {
                                ViewBag.comercial1 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1196") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacomercial1 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1197")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1197" == itemC.idObjeto)
                            {
                                ViewBag.comercial2 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1197") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacomercial2 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1198")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1198" == itemC.idObjeto)
                            {
                                ViewBag.comercial3 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1198") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacomercial3 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1199")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1199" == itemC.idObjeto)
                            {
                                ViewBag.comercial4 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1199") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial4 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacomercial4 = "No";
                            }
                        }
                    }
                    #endregion

                    #region Contabilidad
                    if (item.idObjeto == "1205")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1205" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad1 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1205") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad1 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1206")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1206" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad2 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1206") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad2 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1207")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1207" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad3 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1207") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad3 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1208")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1208" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad4 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1208") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad4 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad4 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1211")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1211" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad5 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1211") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad5 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad5 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1212")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1212" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad6 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1212") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad6 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad6 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1213")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1213" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad7 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1213") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad7 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad7 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1214")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1214" == itemC.idObjeto)
                            {
                                ViewBag.contabilidad8 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1214") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad8 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacontabilidad8 = "No";
                            }
                        }
                    }
                    #endregion

                    #region COMPRAS
                    if (item.idObjeto == "1200")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1200" == itemC.idObjeto)
                            {
                                ViewBag.compra0 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1200") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra0 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra0 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1201")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1201" == itemC.idObjeto)
                            {
                                ViewBag.compra1 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1201") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra1 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1202")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1202" == itemC.idObjeto)
                            {
                                ViewBag.compra2 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1202") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra2 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1203")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1203" == itemC.idObjeto)
                            {
                                ViewBag.compra3 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1203") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra3 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1204")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1204" == itemC.idObjeto)
                            {
                                ViewBag.compra4 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1204") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra4 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra4 = "No";
                            }
                        }
                    }
                    #endregion

                    #region UT
                    if (item.idObjeto == "1215")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1215" == itemC.idObjeto)
                            {
                                ViewBag.ut1 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1215") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaut1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaut1 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1216")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1216" == itemC.idObjeto)
                            {
                                ViewBag.ut2 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1216") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaut2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaut2 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1217")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1217" == itemC.idObjeto)
                            {
                                ViewBag.ut3 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1217") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaut3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaut3 = "No";
                            }
                        }
                    }
                    #endregion

                    #region UER
                    if (item.idObjeto == "1218")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1218" == itemC.idObjeto)
                            {
                                ViewBag.uer1 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1218") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservauer1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservauer1 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1219")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1219" == itemC.idObjeto)
                            {
                                ViewBag.uer2 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1219") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservauer2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservauer2 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1220")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1220" == itemC.idObjeto)
                            {
                                ViewBag.uer3 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1220") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservauer3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservauer3 = "No";
                            }
                        }
                    }
                    #endregion

                }
            }
        }

        public ActionResult IndexPost(string idObjeto, string Fecha, string HoraInicio, string HoraFin, string iduser)
        {
            int hi = Convert.ToInt32(HoraInicio);
            int hf = Convert.ToInt32(HoraFin);
            string usuario = System.Web.HttpContext.Current.Session["IdUser"] as String;
            if (hi > hf)
            {

                Helper.Mensaje = "La hora de entrada es mayor a la hora de salida.";
                Helper.Controller = "/MapaCompleto/Index/";
                return RedirectToAction("Index", "MensajeGenerico");
            }

            int hfitems, hiitems;
            DateTime fechaf;
            DateTime fechaf2 = Convert.ToDateTime(Fecha);
            ConsultaReservas(iduser);
            foreach (var item in reservas)
            {
                hfitems = Convert.ToInt32(item.HoraFin);
                hiitems = Convert.ToInt32(item.HoraInicio);

                fechaf = Convert.ToDateTime(item.Fecha);

                string _fechaFinal = fechaf.ToString("yyyy-MM-dd");

                string _fechaFinal2 = fechaf2.ToString("yyyy-MM-dd");

                if (string.Equals(item.idObjeto, idObjeto) && (string.Equals(_fechaFinal2, _fechaFinal)))
                {

                    if ((hi > hiitems || hf < hfitems))
                    {
                        Helper.Mensaje = "No se pudo registrar la reserva porque el horario ya se encuentra registrado. Hora Inicio: " + item.HoraInicio + " Hora Finalizacion: " + item.HoraFin + " Por: " + item.NombreCompleto;
                        Helper.Controller = "/MapaCompleto/Index/";
                        return RedirectToAction("Index", "MensajeGenerico");
                    }
                    if ((hi < hiitems && hf > hfitems))
                    {
                        Helper.Mensaje = "No se pudo registrar la reserva porque el horario ya se encuentra registrado. Hora Inicio: " + item.HoraInicio + " Hora Finalizacion: " + item.HoraFin + " Por: " + item.NombreCompleto;
                        Helper.Controller = "/MapaCompleto/Index/";
                        return RedirectToAction("Index", "MensajeGenerico");
                    }

                }
            }

            //Insertamos el Elemento.
            var Consulta = "insert into tblReservas(idObjeto,Fecha,HoraInicio,HoraFin,idUsuario) values (@idObjeto,@Fecha,@HoraInicio,@HoraFin,@idUsuario)";

            using (var conexion = new SqlConnection(conexionData))
            {
                conexion.Execute(Consulta, new { idObjeto = idObjeto, Fecha = Fecha, HoraInicio = HoraInicio, HoraFin = HoraFin, idUsuario = usuario });
            }
 
            Helper.Mensaje = "Reserva Ingresada Exitosamente ! ";
            Helper.Controller = "/MapaCompleto/Index/";
            return RedirectToAction("Index", "MensajeGenericoOK");
        }

        public void ConsultaReservas(string iduser)
        {
            var Consulta = "select tr.idObjeto,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,HoraInicio,HoraFin,Fecha from tblreservas tr inner join tblObjetos tob on tr.idObjeto = tob.idObjeto inner join tblArea ta on tob.idArea = ta.idArea inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto inner join tblUsuario tu on tr.idUsuario = tu.idUsuario where tr.idUsuario = " + iduser + " order by idReserva desc";
            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<ConsultaReserva>(Consulta).ToList();

                reservas = Data;

            }
        }

        
    }
}