using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class CSIController : Controller
    {
        // GET: CSI
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
            CorreoElectronico = System.Web.HttpContext.Current.Session["Correo"] as String;
            ViewBag.idPerfil = System.Web.HttpContext.Current.Session["idPerfil"] as String;
            ViewBag.NombrePerfil = System.Web.HttpContext.Current.Session["NombrePerfil"] as String;
            ViewBag.email = System.Web.HttpContext.Current.Session["Correo"] as String;
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

            //Consultamos toda la programación
            ConsultaProgramacion();

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
                    #region CCI-OFICINA
                    if (item.idObjeto == "1226")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1226" == itemC.idObjeto)
                            {
                                ViewBag.CCI1 = complementos + " " + itemC.Complementos;

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



                            if ("1226" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaCC1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC1 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1227")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1227" == itemC.idObjeto)
                            {
                                ViewBag.CCI2 = complementos + " " + itemC.Complementos;

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



                            if ("1227" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaCC2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC2 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1228")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1228" == itemC.idObjeto)
                            {
                                ViewBag.CCI3 = complementos + " " + itemC.Complementos;

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



                            if ("1228" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaCC3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC3 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1229")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1229" == itemC.idObjeto)
                            {
                                ViewBag.CCI4 = complementos + " " + itemC.Complementos;

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



                            if ("1229" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaCC4 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC4 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1230")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1230" == itemC.idObjeto)
                            {
                                ViewBag.CCI5 = complementos + " " + itemC.Complementos;

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



                            if ("1230" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaCC5 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC5 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1231")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1231" == itemC.idObjeto)
                            {
                                ViewBag.CCI6 = complementos + " " + itemC.Complementos;

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



                            if ("1231" == item2.idObjeto && string.Equals(fechahoy, fechareserva) && (hi < ha) && (hf > ha))
                            {
                                ViewBag.reservaCC6 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC6 = "No";
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
                    #region CCI-OFICINA
                    if (item.idObjeto == "1226")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1226" == itemC.idObjeto)
                            {
                                ViewBag.CCI1 = complementos + " " + itemC.Complementos;

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



                            if ("2223" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaCC1 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC1 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1227")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1227" == itemC.idObjeto)
                            {
                                ViewBag.CCI2 = complementos + " " + itemC.Complementos;

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



                            if ("1227" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaCC2 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC2 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1228")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1228" == itemC.idObjeto)
                            {
                                ViewBag.CCI3 = complementos + " " + itemC.Complementos;

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



                            if ("1228" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaCC3 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC3 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1229")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1229" == itemC.idObjeto)
                            {
                                ViewBag.CCI4 = complementos + " " + itemC.Complementos;

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



                            if ("1229" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaCC4 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC4 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1230")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1230" == itemC.idObjeto)
                            {
                                ViewBag.CCI5 = complementos + " " + itemC.Complementos;

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



                            if ("1230" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaCC5 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC5 = "No";
                            }

                        }
                    }

                    if (item.idObjeto == "1231")
                    {
                        complementos = item.Codigo + " " + item.Nombre + " ";
                        foreach (var itemC in ViewBag.DatosMapaCompl)
                        {
                            if ("1231" == itemC.idObjeto)
                            {
                                ViewBag.CCI6 = complementos + " " + itemC.Complementos;

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



                            if ("1231" == item2.idObjeto && string.Equals(fechahoy, fechareserva))
                            {
                                ViewBag.reservaCC6 = "Si";
                                break;
                            }
                            else
                            {
                                ViewBag.reservaCC6 = "No";
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
            string HoraInicio, string HoraFin, string iduser, string almuerzo, string observaciones, string nombrepuesto)
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

            //Consultamos todas reservas de ese puesto para saber si tiene reserva para ese dia seleccionado.
            ConsultaReservas(idObjeto);

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
            if (idObjeto != "1230" && idObjeto != "1231")
            {
                string Consulta = "select Nombre from tblReservas tr " +
                    "inner join tblObjetos tob on tr.idObjeto =tob.idObjeto " +
                    "where idUsuario = " + usuario + " and Fecha = '" + Fecha + "' and tr.idObjeto != 1221 and tr.idObjeto != 1222 and tr.idObjeto != 1224";
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

            await Helper.EnviarConfirmacion(HoraInicio, HoraFin, Fecha, nombrepuesto, CorreoElectronico, observaciones);

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

        public void ConsultaProgramacion()
        {
            string FechaActual = System.DateTime.Now.ToString("yyyyMMdd");

            var Consulta = "select CONVERT(varchar,Fecha,107) as FechaReserva,HoraInicio,HoraFin,Observaciones,Nombre,TipoObjeto from tblReservas tr inner join tblObjetos tob on tob.idObjeto = tr.idObjeto inner join tblTipoObjeto ttob on tob.idTipoObjeto = ttob.idTipoObjeto where Fecha >= '" + FechaActual + "' and (tr.idObjeto=1226 or tr.idObjeto=1227 or tr.idObjeto=1228 or tr.idObjeto=1229 or tr.idObjeto=1230 or tr.idObjeto=1231)";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<MiReservas>(Consulta).ToList();

                if (Data.Count == 0)
                {

                    List<MiReservas> _miReservas = new List<MiReservas>();
                    ViewBag.misReservas = _miReservas;

                }
                else
                {

                    ViewBag.misReservas = Data;

                }

            }
        }
    }
}