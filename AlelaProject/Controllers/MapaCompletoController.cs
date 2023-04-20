using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class MapaCompletoController : Controller
    {
        // GET: MapaCompleto

        private static List<ConsultaReserva> reservas = new List<ConsultaReserva>();
        public static List<HoraAlmuerzo> mishorasAluerzo = new List<HoraAlmuerzo>();
        public static string idUsuario { get; set; }
        public static string CorreoElectronico { get; set; }
        public static string idPerfil { get; set; }

        string conexionData = Helper.Coneccion;

        //[OutputCache(Duration = 60)]
        public ActionResult Index(string FechaBusqueda)
        {
            ViewBag.NombreCompleto = System.Web.HttpContext.Current.Session["NombreCompleto"] as String;
            ViewBag.IdUser = System.Web.HttpContext.Current.Session["IdUser"] as String;
            ViewBag.CorreoElectronico = System.Web.HttpContext.Current.Session["Correo"] as String;
            ViewBag.idPerfil = System.Web.HttpContext.Current.Session["idPerfil"] as String;
            ViewBag.NombrePerfil = System.Web.HttpContext.Current.Session["NombrePerfil"] as String;
            ViewBag.urlima = System.Web.HttpContext.Current.Session["urlima"] as String;

            if (string.IsNullOrEmpty(ViewBag.NombreCompleto))
            {
                return RedirectToAction("Index", "Home");
            }
            if (string.IsNullOrEmpty(FechaBusqueda))
            {
                ConsultaMapa();
                ViewBag.FechaConsulta = System.DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                ConsultaMapaFecha(FechaBusqueda);
                ViewBag.FechaConsulta = FechaBusqueda;
            }

            ConsultaHoras();
            ConsultaDia();
            ConsultaHoraAlmuerzo();
            ViewBag.cod = 0;
            //ConsultaReservas(ViewBag.IdUser);
            return View();

        }
        public void ConsultaMapa()
        {
            string conexionData = Helper.Coneccion;
            string Consulta = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto";
            string ConsultaComple = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea,NombreComplemento  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto inner join tblObjetoComplemento toc on tob.idobjeto = toc.idObjeto left join tblComplemento tc on toc.idcomplemento = tc.idComplemento";
            string ConsultaReservas = "select idObjeto, Fecha,HoraInicio,HoraFin,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,NombreHora,observaciones from tblReservas tr inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario left join tblHoraAlmuerzo tha on tr.idHoraAlmuerzo = tha.idHora order by Fecha asc";

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
                        item.NombreDia = item.Fecha.ToString("dddd");
                        if (item.Fecha > DateTime.Now)
                        {
                            item.Disponible = "Pendiente";
                        }
                        else if (item.Fecha.Date == DateTime.Now.Date)
                        {
                            item.Disponible = "Hoy";
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);



                            if ("1209" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.ReservaActual = "Si";

                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1209 = item2.NombreCompleto;
                                }

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
                                ViewBag.PuestoOscar = complementos + " " + itemC.Complementos + "<br>";
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);


                            if ("1210" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.ReservaActualOscar = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1210 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);


                            if (string.Equals(item2.idObjeto, "1190") && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservagescomer1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1190 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            if ("1191" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservagescomer2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1191 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            if ("1192" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservagescomer3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1192 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1193" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservagescomer4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1193 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1194" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservagescomer5 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1194 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1195" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservagescomer6 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1195 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1196") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacomercial1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1196 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1197") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacomercial2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1197 = item2.NombreCompleto;
                                }

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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1198") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacomercial3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1198 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1199") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacomercial4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1199 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1205") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1205 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1206") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1206 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1207") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1207 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1208") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1208 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1211") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad5 = "Si";

                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1211 = item2.NombreCompleto;
                                }

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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1212") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad6 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1212 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1213") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad7 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1213 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1214") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacontabilidad8 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1214 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1200") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacompra0 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1200 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1201") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacompra1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1201 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1202") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacompra2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1202 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1203") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacompra3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1203 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1204") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacompra4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1204 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra4 = "No";
                            }
                        }
                    }
                    if (item.idObjeto == "1225")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1225" == itemC.idObjeto)
                            {
                                ViewBag.compra5 = complementos + " " + itemC.Complementos;
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1225") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservacompra5 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1225 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra5 = "No";
                            }
                        }
                    }

                    #endregion

                    #region Auditorios

                    if (item.idObjeto == "1221")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1221" == itemC.idObjeto)
                            {
                                ViewBag.salajunta1 = complementos + " " + itemC.Complementos;

                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);



                            if ("1221" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.Reservasalajunta1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1221 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.Reservasalajunta1 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1222")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";

                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1222" == itemC.idObjeto)
                            {
                                ViewBag.Auditorio = complementos + " " + itemC.Complementos;

                            }
                        }

                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);



                            if ("1222" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.Reservaauditorio = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1222 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.Reservaauditorio = "No";
                            }
                        }
                    }

                    #endregion
                }
            }


        }
        public void ConsultaMapaFecha(string FechaBusqueda)
        {
            string conexionData = Helper.Coneccion;
            var Consulta = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto";
            var ConsultaComple = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea,NombreComplemento  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto inner join tblObjetoComplemento toc on tob.idobjeto = toc.idObjeto left join tblComplemento tc on toc.idcomplemento = tc.idComplemento";
            var ConsultaReservas = "select idObjeto, Fecha,HoraInicio,HoraFin,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,NombreHora from tblReservas tr inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario left join tblHoraAlmuerzo tha on tr.idHoraAlmuerzo = tha.idHora order by Fecha asc";

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
                        item.NombreDia = item.Fecha.ToString("dddd");
                        if (item.Fecha.Date > System.DateTime.Now.Date)
                        {
                            item.Disponible = "No Vencido";
                        }
                        else if (item.Fecha.Date == System.DateTime.Now.Date)
                        {
                            item.Disponible = "Hoy";
                        }
                        else
                        {
                            item.Disponible = "Vencido";
                        }
                    }
                    ViewBag.DatosReservas = DataReser;
                }

                //Buscamos si cada puesto tiene reserva activa
                string fechahoy = FechaBusqueda;
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);



                            if ("1209" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.ReservaActual = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1209 = item2.NombreCompleto;
                                }
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
                                ViewBag.PuestoOscar = complementos + " " + itemC.Complementos + "<br>";
                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);


                            if ("1210" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.ReservaActualOscar = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1210 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);


                            if (string.Equals(item2.idObjeto, "1190") && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1190 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            if ("1191" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1191 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            if ("1192" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1192 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1193" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1193 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1194" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer5 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1194 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if ("1195" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservagescomer6 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1195 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1196") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1196 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1197") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1197 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1198") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1198 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1199") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacomercial4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1199 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1205") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1205 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1206") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1206 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1207") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1207 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1208") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1208 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1212") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad6 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1212 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1213") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad7 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1213 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1214") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacontabilidad8 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1214 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1200") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra0 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1200 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1201") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1201 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1202") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1202 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1203") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1203 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1204") &&
                                string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservacompra4 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1204 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.reservacompra4 = "No";
                            }
                        }
                    }
                    #endregion

                    #region SALA JUNTAS
                    if (item.idObjeto == "1221")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1221" == itemC.idObjeto)
                            {
                                ViewBag.salajunta1 = complementos + " " + itemC.Complementos;

                            }
                        }
                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);



                            if ("1221" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.Reservasalajunta1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1221 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.Reservasalajunta1 = "No";
                            }

                        }
                    }
                    if (item.idObjeto == "1222")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";

                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1222" == itemC.idObjeto)
                            {
                                ViewBag.Auditorio = complementos + " " + itemC.Complementos;

                            }
                        }

                        foreach (var item2 in DataReser)
                        {
                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;

                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);



                            if ("1222" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.Reservaauditorio = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1222 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.Reservasauditorio = "No";
                            }

                        }
                    }
                    #endregion
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
        public void ConsultaDia()
        {
            List<Dia> dias = new List<Dia>();
            dias.Add(new Dia { NombreDia = "lunes" });
            dias.Add(new Dia { NombreDia = "martes" });
            dias.Add(new Dia { NombreDia = "miércoles" });
            dias.Add(new Dia { NombreDia = "jueves" });
            dias.Add(new Dia { NombreDia = "viernes" });

            ViewBag.Dias = dias;
        }
        public async Task<ActionResult> IndexPost(string idObjeto, string Fecha,
            string HoraInicio, string HoraFin, string iduser, string almuerzo, string observaciones, string nombrepuesto, string Email)
        {
            int hi = Convert.ToInt32(HoraInicio);
            int hf = Convert.ToInt32(HoraFin);
            string usuario = System.Web.HttpContext.Current.Session["IdUser"] as String;

            if (observaciones == "undefined")
            {
                observaciones = string.Empty;
            }

            //Verficamos que la fecha actual sea mayor o igual que la fecha actual.
            DateTime fechaA = System.DateTime.Today;
            DateTime fechaR = Convert.ToDateTime(Fecha);
            if (fechaR < fechaA)
            {
                Helper.Mensaje = "La fecha de la reserva es anterior a la fecha actual. Verificar";
                Helper.Controller = "/MapaCompleto/Index/";
                return RedirectToAction("Index", "MensajeGenerico");
            }

            if (hi > hf)
            {

                Helper.Mensaje = "La hora de entrada es mayor a la hora de salida.";
                Helper.Controller = "/MapaCompleto/Index/";
                return RedirectToAction("Index", "MensajeGenerico");
            }

            int hfitems, hiitems;
            DateTime fechaf;
            DateTime fechaf2 = Convert.ToDateTime(Fecha);

           
            ConsultaReservas(idObjeto);

            if (!string.Equals(idObjeto, 1222) || !string.Equals(idObjeto, 1221))
            {
                //Consultamos todas reservas de ese puesto para saber si tiene reserva para ese dia seleccionado.
                var reservaspendinete = ConsultaReservas(idObjeto, System.DateTime.Now.ToString("yyyyMMdd"));
                if (reservaspendinete.Count > 5)
                {
                    Helper.Mensaje = "Numero de reservas supera el limite permitido: 5 reservas pendientes maximo por espacio.";
                    Helper.Controller = "/MapaCompleto/Index/";
                    return RedirectToAction("Index", "MensajeGenerico");
                }
            }

        

            foreach (var item in reservas)
            {
                hfitems = Convert.ToInt32(item.HoraFin);
                hiitems = Convert.ToInt32(item.HoraInicio);

                fechaf = Convert.ToDateTime(item.Fecha);

                string _fechaFinal = fechaf.ToString("yyyy-MM-dd");

                string _fechaFinal2 = fechaf2.ToString("yyyy-MM-dd");

                if (string.Equals(item.idObjeto, idObjeto) &&
                    (string.Equals(_fechaFinal2, _fechaFinal)))
                {

                    if ((hi >= hiitems && hi <= hfitems))
                    {
                        Helper.Mensaje = "No se pudo registrar la reserva porque el horario ya se encuentra registrado. Hora Inicio: " + item.HoraInicio + " Hora Finalizacion: " + item.HoraFin + " Por: " + item.NombreCompleto;
                        Helper.Controller = "/MapaCompleto/Index/";
                        return RedirectToAction("Index", "MensajeGenerico");
                    }
                    if ((hf >= hfitems && hf <= hfitems))
                    {
                        Helper.Mensaje = "No se pudo registrar la reserva porque el horario ya se encuentra registrado. Hora Inicio: " + item.HoraInicio + " Hora Finalizacion: " + item.HoraFin + " Por: " + item.NombreCompleto;
                        Helper.Controller = "/MapaCompleto/Index/";
                        return RedirectToAction("Index", "MensajeGenerico");
                    }
                    if ((hf <= hiitems && hf >= hfitems))
                    {
                        Helper.Mensaje = "No se pudo registrar la reserva porque el horario ya se encuentra registrado. Hora Inicio: " + item.HoraInicio + " Hora Finalizacion: " + item.HoraFin + " Por: " + item.NombreCompleto;
                        Helper.Controller = "/MapaCompleto/Index/";
                        return RedirectToAction("Index", "MensajeGenerico");
                    }
                    if ((hi <= hiitems && hf >= hfitems))
                    {
                        Helper.Mensaje = "No se pudo registrar la reserva porque el horario ya se encuentra registrado. Hora Inicio: " + item.HoraInicio + " Hora Finalizacion: " + item.HoraFin + " Por: " + item.NombreCompleto;
                        Helper.Controller = "/MapaCompleto/Index/";
                        return RedirectToAction("Index", "MensajeGenerico");
                    }

                }
            }

            //Verificamos que el usuario no tenga reserva en otro puesto
            if (idObjeto != "1221" && idObjeto != "1222" && idObjeto != "1224" &&
                idObjeto != "1230" && idObjeto != "1231" && idObjeto != "1226" &&
                idObjeto != "1227" && idObjeto != "1228" && idObjeto != "1229")
            {
                string Consulta = "select Nombre from tblReservas tr " +
                    "inner join tblObjetos tob on tr.idObjeto =tob.idObjeto " +
                    "where idUsuario = " + usuario + " and Fecha = '" + Fecha + "' and tr.idObjeto != 1221 and tr.idObjeto != 1222 and tr.idObjeto != 1224 and tr.idObjeto != 1230 and tr.idObjeto != 1231";
                using (var conexion = new SqlConnection(conexionData))
                {
                    var _datos = conexion.Query<MiReservas>(Consulta).ToList();

                    if (_datos.Count != 0)
                    {
                        Helper.Mensaje = "No se puede reservar porque ya tiene una reserva en " + _datos[0].Nombre + "Para la fecha " + Fecha;
                        Helper.Controller = "/MapaCompleto/Index/";
                        return RedirectToAction("Index", "MensajeGenerico");
                    }
                }

            }

            //Insertamos el Elemento.
            using (var conexion = new SqlConnection(conexionData))
            {
                //usamos un procedimiento almacenado para insertar la reserva
                var procedimiento = "IngresarReserva";

                var parametros = new DynamicParameters();
                parametros.Add("idObjeto", idObjeto);
                parametros.Add("Fecha", Fecha);
                parametros.Add("HoraInicio", HoraInicio);
                parametros.Add("HoraFin", HoraFin);
                parametros.Add("idUsuario", usuario);
                parametros.Add("observaciones", "OBS: " + observaciones.ToUpper());
                parametros.Add("idHoraAlmuerzo", almuerzo);
                parametros.Add("id", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 50);

                conexion.Execute(procedimiento, parametros, commandType: System.Data.CommandType.StoredProcedure);

                var _idRespuesta = parametros.Get<object>("id");

                if (_idRespuesta.ToString() == "0")
                {
                    Helper.Mensaje = "No se pudo registrar la reserva porque el horario ya se encuentra registrado. Verificar las reservas de este puesto";
                    Helper.Controller = "/MapaCompleto/Index/";
                    return RedirectToAction("Index", "MensajeGenerico");
                }
            }

            await Helper.EnviarConfirmacion(HoraInicio, HoraFin, Fecha, nombrepuesto, Email, observaciones);

            Helper.Mensaje = "Reserva Ingresada Exitosamente ! ";

            Helper.Controller = "/MapaCompleto/Index/";

            return RedirectToAction("Index", "MensajeGenericoOK");
        }
        public void ConsultaReservas(string idObjeto)
        {

            var Consulta = "select tr.idObjeto,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,HoraInicio,HoraFin,Fecha from tblreservas tr inner join tblObjetos tob on tr.idObjeto = tob.idObjeto inner join tblArea ta on tob.idArea = ta.idArea inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario where tr.idObjeto = " + idObjeto + " order by idReserva desc";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<ConsultaReserva>(Consulta).ToList();

                foreach (var item in Data)
                {
                    if (item.Fecha.Date > System.DateTime.Now.Date)
                    {
                        item.Disponible = "No Vencido";
                    }
                    else if (item.Fecha.Date == System.DateTime.Now.Date)
                    {
                        item.Disponible = "Hoy";
                    }
                    else
                    {
                        item.Disponible = "Vencido";
                    }
                }

                reservas = Data;

            }
        }
        public List<ConsultaReserva> ConsultaReservas(string idObjeto, string Fecha)
        {

            var Consulta = "select tr.idObjeto,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,HoraInicio,HoraFin,Fecha from tblreservas tr inner join tblObjetos tob on tr.idObjeto = tob.idObjeto inner join tblArea ta on tob.idArea = ta.idArea inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario where tr.idObjeto = " + idObjeto + " and Fecha > '" + Fecha + "' order by idReserva desc";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<ConsultaReserva>(Consulta).ToList();

                foreach (var item in Data)
                {
                    if (item.Fecha.Date > System.DateTime.Now.Date)
                    {
                        item.Disponible = "No Vencido";
                    }
                    else if (item.Fecha.Date == System.DateTime.Now.Date)
                    {
                        item.Disponible = "Hoy";
                    }
                    else
                    {
                        item.Disponible = "Vencido";
                    }
                }

                return Data;

            }
        }
    }
}