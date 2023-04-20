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
    public class ReportesAsistenciasController : Controller
    {
        public static string CorreoElectronico { get; set; }
        string conexionData = Helper.Coneccion;
        // GET: ReportesAsistencias
        public ActionResult Index(string Ano, string Mes)
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

            string id = System.Web.HttpContext.Current.Session["IdUser"] as String;

            List<Asistente> DataAsistentes;
            List<Asistente> ListaFinal = new List<Asistente>();

            var Consulta = "select (tp.emp_code),first_name,punch_time,passport,CONVERT(varchar,punch_time,107) as fecha from zkbiotime..iclock_transaction  " +
                "tt right join zkbiotime..personnel_employee tp on tt.emp_code = tp.emp_code " +
                "where  YEAR(punch_time) = " + System.DateTime.Now.Year + " and month(punch_time) = " + System.DateTime.Now.Month + " ORDER BY fecha,first_name";

            if ( !string.IsNullOrEmpty(Ano) && !string.IsNullOrEmpty(Mes))
            {
                Consulta = "select (tp.emp_code),first_name,punch_time,passport,CONVERT(varchar,punch_time,107) as fecha from zkbiotime..iclock_transaction  " +
                "tt right join zkbiotime..personnel_employee tp on tt.emp_code = tp.emp_code " +
                "where  YEAR(punch_time) = " + Ano + " and month(punch_time) = " + Mes + " ORDER BY fecha,first_name";
            }

            List<Evento> dias = new List<Evento>();



            using (var conexion = new SqlConnection(conexionData))
            {
                DataAsistentes = conexion.Query<Asistente>(Consulta).ToList();
            }

            foreach (var item in DataAsistentes)
            {
                var salida = DataAsistentes.OrderByDescending(x => x.fecha).Last(x => x.passport == item.passport && x.fecha == item.fecha);
                var llegada = DataAsistentes.OrderByDescending(x => x.fecha).First(x => x.passport == item.passport && x.fecha == item.fecha);

                if (salida.punch_time == llegada.punch_time)
                {
                    item.variable = "S";
                }

                ListaFinal.Add(new Asistente
                {
                    first_name = item.first_name,
                    fecha = item.punch_time.ToString("yyyy-MMM-dd dddd"),
                    Llegada = llegada.punch_time.ToString("HH:mm:ss"),
                    Salida = salida.punch_time.ToString("HH:mm:ss"),
                    variable = item.variable
                });
            }

            IEnumerable<Asistente> Datos = ListaFinal.GroupBy(x => new { x.first_name, x.fecha }).Select(y => y.First()).ToList();


            if (Datos.Count() > 0)
            {
                ViewBag.DataAsistentes = Datos;
            }
            else
            {
                ViewBag.DataAsistentes = new List<Asistente>();
            }
            ConsultaPeriodo();
            ConsultaMes();
            return View();
        }
        public void ConsultaPeriodo()
        {

            var Consulta = "select distinct YEAR(punch_time) as anio from zkbiotime..iclock_transaction ";

            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<AñoMes>(Consulta).ToList();

                if (Data.Count == 0)
                {
                    ViewBag.periodo = null;
                }
                else
                {
                    List<SelectListItem> item = Data.ConvertAll(d =>
                    {
                        return new SelectListItem()
                        {
                            Text = d.anio.ToString(),
                            Value = d.anio.ToString(),
                            Selected = false
                        };
                    });

                    ViewBag.anio = item;
                }
            }
        }

        public void ConsultaMes()
        {

            List<AñoMes> _mes = new List<AñoMes>();
            _mes.Add(new AñoMes { mes = "1" });
            _mes.Add(new AñoMes { mes = "2" });
            _mes.Add(new AñoMes { mes = "3" });
            _mes.Add(new AñoMes { mes = "4" });
            _mes.Add(new AñoMes { mes = "5" });
            _mes.Add(new AñoMes { mes = "6" });
            _mes.Add(new AñoMes { mes = "7" });
            _mes.Add(new AñoMes { mes = "8" });
            _mes.Add(new AñoMes { mes = "9" });
            _mes.Add(new AñoMes { mes = "10" });
            _mes.Add(new AñoMes { mes = "11" });
            _mes.Add(new AñoMes { mes = "12" });




            List<SelectListItem> item = _mes.ConvertAll(d =>
            {
                return new SelectListItem()
                {
                    Text = d.mes.ToString(),
                    Value = d.mes.ToString(),
                    Selected = false
                };
            });
            ViewBag.Mes = item;
        }
    }
}