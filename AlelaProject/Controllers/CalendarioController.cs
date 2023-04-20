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
    public class CalendarioController : Controller
    {
        // GET: Calendario
        public static string CorreoElectronico { get; set; }
        string conexionData = Helper.Coneccion;
        public ActionResult Index()
        {
            ViewBag.NombreCompleto = System.Web.HttpContext.Current.Session["NombreCompleto"] as String;
            ViewBag.IdUser = System.Web.HttpContext.Current.Session["IdUser"] as String;
            CorreoElectronico = System.Web.HttpContext.Current.Session["Correo"] as String; ;
            ViewBag.idPerfil = System.Web.HttpContext.Current.Session["idPerfil"] as String; ;
            ViewBag.NombrePerfil = System.Web.HttpContext.Current.Session["NombrePerfil"] as String;
            ViewBag.urlima = System.Web.HttpContext.Current.Session["urlima"] as String;

            if (string.IsNullOrEmpty(ViewBag.NombreCompleto))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public JsonResult GetEvents()
        {
            string id = System.Web.HttpContext.Current.Session["IdUser"] as String;
            List<Asistente> DataAsistentes;
            List<MiReservas> ReservasHoy;

            //var Consulta = "select Fecha,HoraInicio,HoraFin,Observaciones,Nombre,Codigo,NombreArea from tblReservas tr " +
            //                "inner join tblObjetos tob on tr.idObjeto = tob.idObjeto " +
            //                "inner join tblArea ta on tob.idArea = ta.idArea where idUsuario = " + id;

            var Consulta = "  select tu.idUsuario, Fecha,HoraInicio,HoraFin,Observaciones,Nombre,Codigo,NombreArea,NombreCompleto,Contrasena from tblReservas tr " +
                            " inner join tblObjetos tob on tr.idObjeto = tob.idObjeto " +
                            " inner join tblArea ta on tob.idArea = ta.idArea " +
                            " inner join DT_ControlUsuarios..tblusuarios tu on tr.idUsuario = tu.idUsuario";
            List<Evento> dias = new List<Evento>();
            var fech = System.DateTime.Now.Date.ToString("yyyyMMdd");
            var fechCons = System.DateTime.Now.Date.ToString("yyyy-MM-dd");

            var Consulta2 = "select (tp.emp_code),first_name,last_name,punch_time,passport from zkbiotime..iclock_transaction  tt right join zkbiotime..personnel_employee tp on tt.emp_code = tp.emp_code where punch_time >= '" + fech + "' order by emp_code";


            var Consulta3 = " select tu.idUsuario, Fecha,HoraInicio,HoraFin,Observaciones,Nombre,Codigo,NombreArea,NombreCompleto,Contrasena from tblReservas tr " +
                            " inner join tblObjetos tob on tr.idObjeto = tob.idObjeto " +
                            " inner join tblArea ta on tob.idArea = ta.idArea " +
                            " inner join DT_ControlUsuarios..tblusuarios tu on tr.idUsuario = tu.idUsuario where Fecha = '" + fechCons + "'";

            using (var conexion = new SqlConnection(conexionData))
            {
                ReservasHoy = conexion.Query<MiReservas>(Consulta3).ToList();
            }

            using (var conexion = new SqlConnection(conexionData))
            {
                DataAsistentes = conexion.Query<Asistente>(Consulta2).ToList();

                foreach (var item in DataAsistentes)
                {
                    item.passport = Helper.Encryptation(item.passport);
                }
            }

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<MiReservas>(Consulta).ToList();

                foreach (var item in Data)
                {
                    var fecha = Convert.ToDateTime(item.Fecha);
                    var horai = item.HoraInicio.Substring(0, 2);
                    var minutosi = item.HoraInicio.Substring(2, 2);

                    var horaf = item.HoraFin.Substring(0, 2);
                    var minutosf = item.HoraFin.Substring(2, 2);

                    var start = fecha.ToString("dd/MM/yyyy") + " " + horai + ":" + minutosi;
                    var end = fecha.ToString("dd/MM/yyyy") + " " + horaf + ":" + minutosf;

                    DateTime fechainicio = Convert.ToDateTime(start);
                    DateTime fechafin = Convert.ToDateTime(end);
                    if (item.Fecha != System.DateTime.Now.Date)
                    {
                        dias.Add(new Evento
                        {
                            Subject = item.Nombre + "-" + item.NombreCompleto,
                            Start = fechainicio,
                            End = fechafin,
                            Descripcion = item.NombreArea,
                            IsFullDay = false,
                            ThemeColor = "#ECFAEE",
                            NombreCompleto = item.NombreCompleto,
                        });
                    }

                }

            }

            foreach (var item in DataAsistentes)
            {
                foreach (var item2 in ReservasHoy)
                {
                    if (item.passport == item2.Contrasena)
                    {
                        item2.Asistio = "Si";
                        item2.Reservo = "Si";

                        item.Asistio = "Si";
                        item.Reservo = "Si";
                    }
                }
            }

            foreach (var item in ReservasHoy)
            {
                var fecha = Convert.ToDateTime(item.Fecha);
                var horai = item.HoraInicio.Substring(0, 2);
                var minutosi = item.HoraInicio.Substring(2, 2);

                var horaf = item.HoraFin.Substring(0, 2);
                var minutosf = item.HoraFin.Substring(2, 2);

                var start = fecha.ToString("dd/MM/yyyy") + " " + horai + ":" + minutosi;
                var end = fecha.ToString("dd/MM/yyyy") + " " + horaf + ":" + minutosf;

                DateTime fechainicio = Convert.ToDateTime(start);
                DateTime fechafin = Convert.ToDateTime(end);


                if (item.Reservo == "Si" && item.Asistio == "Si")
                {
                    dias.Add(new Evento
                    {
                        Subject = item.Nombre + "-" + item.NombreCompleto,
                        Start = fechainicio,
                        End = fechafin,
                        Descripcion = item.NombreArea,
                        IsFullDay = false,
                        ThemeColor = "#AAFF99",
                        NombreCompleto = item.NombreCompleto,
                    });
                }
                if (string.IsNullOrEmpty(item.Reservo) &&
                    string.IsNullOrEmpty(item.Asistio))
                {
                    dias.Add(new Evento
                    {
                        Subject = item.Nombre + "-" + item.NombreCompleto,
                        Start = fechainicio,
                        End = fechafin,
                        Descripcion = item.NombreArea,
                        IsFullDay = false,
                        ThemeColor = "#FFA793",
                        NombreCompleto = item.NombreCompleto,
                    });
                }



            }

            foreach (var item in DataAsistentes)
            {
                var fecha = System.DateTime.Now;
                var horai = item.punch_time.ToString("HH");
                var minutosi = item.punch_time.ToString("mm");

                var horaf = "17";
                var minutosf = "00";

                var start = fecha.ToString("dd/MM/yyyy") + " " + horai + ":" + minutosi;
                var end = fecha.ToString("dd/MM/yyyy") + " " + horaf + ":" + minutosf;

                DateTime fechainicio = Convert.ToDateTime(start);
                DateTime fechafin = Convert.ToDateTime(end);


                if (item.Reservo == null && item.Asistio == null)
                {
                    dias.Add(new Evento
                    {
                        Subject = "Diario -" + item.first_name,
                        Start = item.punch_time,
                        End = fechafin,
                        Descripcion = "Tiempo completo",
                        IsFullDay = false,
                        ThemeColor = "#EBFF93",
                        NombreCompleto = item.first_name,
                    });
                }
            }
            //break;
            return new JsonResult { Data = dias.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }
    }
}