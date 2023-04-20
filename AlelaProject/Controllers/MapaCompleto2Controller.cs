using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class MapaCompleto2Controller : Controller
    {
        private static List<ConsultaReserva> reservas = new List<ConsultaReserva>();
        public static List<HoraAlmuerzo> mishorasAluerzo = new List<HoraAlmuerzo>();
        public static string idUsuario { get; set; }

        string conexionData = Helper.Coneccion;
        // GET: MapaCompleto2
        public ActionResult Index(string FechaBusqueda)
        {
            ViewBag.NombreCompleto = System.Web.HttpContext.Current.Session["NombreCompleto"] as String;
            ViewBag.IdUser = System.Web.HttpContext.Current.Session["IdUser"] as String;
            ViewBag.idPerfil = System.Web.HttpContext.Current.Session["idPerfil"] as String; ;
            ViewBag.NombrePerfil = System.Web.HttpContext.Current.Session["NombrePerfil"] as String;
            ViewBag.CorreoElectronico = System.Web.HttpContext.Current.Session["Correo"] as String;
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
            return View();
        }
        public void ConsultaMapa()
        {
            string conexionData = Helper.Coneccion;
            var Consulta = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto";
            var ConsultaComple = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea,NombreComplemento  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto inner join tblObjetoComplemento toc on tob.idobjeto = toc.idObjeto left join tblComplemento tc on toc.idcomplemento = tc.idComplemento";
            var ConsultaReservas = "select idObjeto, Fecha,HoraInicio,HoraFin,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,NombreHora,ISNULL(Observaciones,' ') as Observaciones from tblReservas tr inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario inner join tblHoraAlmuerzo tha on tr.idHoraAlmuerzo = tha.idHora order by Fecha asc";

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
                        if (item.Fecha.Date > System.DateTime.Now.Date)
                        {
                            item.Disponible = "Pendiente";
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
                string fechahoy = DateTime.Today.ToString("yyyy-MM-dd");
                string complementos = string.Empty;
                foreach (var item in Data)
                {

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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1215") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaut1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1215 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1216") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaut2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1216 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1217") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaut3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1217 = item2.NombreCompleto;
                                }
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

                            string fechareserva = Convert.ToDateTime(item2.Fecha).ToString("yyyy-MM-dd");
                            if (string.Equals(item2.idObjeto, "1218") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservauer1 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1218 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.reservauer1 = "No";
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
                                ViewBag.uer2 = complementos + " " + itemC.Complementos;
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
                            if (string.Equals(item2.idObjeto, "1220") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservauer2 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva1220 = item2.NombreCompleto;
                                }
                                break;
                            }
                            else
                            {
                                ViewBag.reservauer2 = "No";
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
                                ViewBag.uer3 = complementos + " " + itemC.Complementos;
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
                            if (string.Equals(item2.idObjeto, "1219") &&
                                string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservauer3 = "Si";
                                if (item2.NombreCompleto == ViewBag.NombreCompleto)
                                {
                                    ViewBag.nombreReserva = item2.NombreCompleto;
                                }
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
        public void ConsultaMapaFecha(string FechaBusqueda)
        {
            string conexionData = Helper.Coneccion;
            var Consulta = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto";
            var ConsultaComple = "Select tob.idObjeto,Nombre,Codigo,NombreArea,TipoObjeto,tob.idArea,NombreComplemento  from tblObjetos tob inner join tblarea ta on tob.idarea = ta.idarea  inner join tbltipoObjeto tto on tob.idtipoobjeto = tto.idtipoobjeto inner join tblObjetoComplemento toc on tob.idobjeto = toc.idObjeto left join tblComplemento tc on toc.idcomplemento = tc.idComplemento";
            var ConsultaReservas = "select idObjeto, Fecha,HoraInicio,HoraFin,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,NombreHora,ISNULL(Observaciones,' ') as Observaciones from tblReservas tr inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario inner join tblHoraAlmuerzo tha on tr.idHoraAlmuerzo = tha.idHora order by Fecha asc";

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
                        if (item.Fecha.Date > System.DateTime.Now.Date)
                        {
                            item.Disponible = "Pendiente";
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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

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
                            string horaactual = System.DateTime.Now.ToString("HHmm");
                            int hi, hf, ha;
                            hi = Convert.ToInt32(item2.HoraInicio);
                            hf = Convert.ToInt32(item2.HoraFin);
                            ha = Convert.ToInt32(horaactual);

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
                    if (item.idObjeto == "1220")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1220" == itemC.idObjeto)
                            {
                                ViewBag.uer2 = complementos + " " + itemC.Complementos;
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
                            if (string.Equals(item2.idObjeto, "1220") &&
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
                    if (item.idObjeto == "1219")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1219" == itemC.idObjeto)
                            {
                                ViewBag.uer3 = complementos + " " + itemC.Complementos;
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
                            if (string.Equals(item2.idObjeto, "1219") &&
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
    }
}