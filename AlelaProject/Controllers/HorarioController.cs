using AlelaProject.Models;
using AlelaProject.Servicio;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AlelaProject.Controllers
{
    public class HorarioController : Controller
    {
        private static List<ConsultaReserva> reservas = new List<ConsultaReserva>();
        public static string CorreoElectronico { get; set; }
        string conexionData = Helper.Coneccion;
        public string id { get; set; }
        public int idc { get; set; }
        // GET: Horario
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
            ConsultaDia();
            ConsultaHoras();
            ConsultaPuestos();
            ConsultaReservas();
            return View();
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
        //Consultamos los puestos de trabajo para el modal
        public void ConsultaPuestos()
        {

            var Consulta = "select idObjeto,Nombre +' ('+ NombreArea +')' as Nombre,Codigo,TipoObjeto  from tblObjetos tob inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto inner join tblArea ta on tob.idArea = ta.idArea";

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

                    ViewBag.DataPuestos = item;
                }
            }
        }
        public async Task<ActionResult> IndexAceptarmuliple(string[] ids, string fechainicial, string fechafin,
            string horainicio, string horafin, string Puesto, string usuarios, string Almuerzo)
        {
            //Recorremos desde la fecha inicio hasta la fecha final
            var _fechainicial = Convert.ToDateTime(fechainicial);

            var _fechafinal = Convert.ToDateTime(fechafin);

            CorreoElectronico = System.Web.HttpContext.Current.Session["Correo"] as String;

            bool entro = false;
            //verificamos que entre una fecha a otra no hay mas de 5 dias de diferencia.
            TimeSpan difFechas = _fechafinal - _fechainicial;
            int dias = difFechas.Days;

            if (dias > 5)
            {
                await Helper.ErrorReserva(fechainicial, fechafin, Puesto, CorreoElectronico);
                Helper.Mensaje = "La diferencia entre una fecha y otra no puede ser superior a 5 dias.";
                Helper.Controller = "/MapaCompleto/Index/";
                return RedirectToAction("Index", "MensajeGenericoERROR");
            }

            ConsultaReservas();

            //VERIFICAMOS QUE EL PUESTO NO TENGA MAS DE 5 RESERVAS PENDIENTE
            if (!string.Equals(Puesto, 1222) || !string.Equals(Puesto, 1221))
            {
                //Consultamos todas reservas de ese puesto para saber si tiene reserva para ese dia seleccionado.
                var reservaspendinete = ConsultaReservas(Puesto, System.DateTime.Now.ToString("yyyyMMdd"));
                if (reservaspendinete.Count > 5)
                {
                    Helper.Mensaje = "Numero de reservas supera el limite permitido: 5 reservas pendientes maximo por espacio.";
                    Helper.Controller = "/MapaCompleto/Index/";
                    return RedirectToAction("Index", "MensajeGenerico");
                }
            }

            // VERIFICAMOS QUE LAS FECHAS NO TENGA RESERVAS EN ESE PUESTO POR OTRA PERSONA.
            var Resultadovalidacion = verificacionhorarioreserva(Puesto, _fechainicial, _fechafinal);

            if (Resultadovalidacion == true)
            {
                await Helper.ErrorReserva(fechainicial, fechafin, Puesto, CorreoElectronico);
                Helper.Mensaje = "Proceso Finalizado pero hubieron fechas que no se pudo reservar debido a que ya se encuentran reservadas, REVISAR EL CORREO DE CONFIRMACION !";
                Helper.Controller = "/MapaCompleto/Index/";
                return RedirectToAction("Index", "MensajeGenericoERROR");
            }

            int contador = 0;

            int _contador = 0;

            bool reservado = false;

            string fechanoreserva = string.Empty;

            string _horaalmuerzo = string.Empty;

            var misreservasHoraAlmuerzo = MapaCompletoController.mishorasAluerzo;

            while (_fechainicial <= _fechafinal)
            {
                //Verificamos si el horario seleccionado tiene reservas
                int hfitems, hiitems;

                DateTime fechaf;

                DateTime fechaf2 = Convert.ToDateTime(_fechainicial);

                //var _usuario = System.Web.HttpContext.Current.Session["IdUser"] as String;

                //Obtenemos el nombre del dia
                string nombredia = _fechainicial.ToString("dddd");

                //Verficamos que la fecha actual sea mayor o igual que la fecha actual.
                DateTime fechaA = System.DateTime.Today;

                if (_fechainicial < fechaA)
                {
                    fechanoreserva = fechanoreserva + " " + nombredia + " " + _fechainicial.ToString("yyyy-MM-dd") + " es anterior a la fecha actual, ";

                    entro = true;

                    reservado = true;
                }

                //VALIDAMOS QUE EL USUARIO NO TENGA RESERVA PARA LA FECHA SELECCIONADA
                var resultadoreservas = TieneReserva(usuarios, _fechainicial);

                if (resultadoreservas == true)
                {
                    fechanoreserva = fechanoreserva + nombredia + " " + _fechainicial + " <br>";
                    entro = true;
                    _fechainicial += new TimeSpan(1, 0, 0, 0);
                    continue;
                }

                foreach (var item in reservas)
                {
                    hfitems = Convert.ToInt32(item.HoraFin);

                    hiitems = Convert.ToInt32(item.HoraInicio);

                    fechaf = Convert.ToDateTime(item.Fecha);

                    string _fechaFinal = fechaf.ToString("yyyy-MM-dd");

                    string _fechaFinal2 = fechaf2.ToString("yyyy-MM-dd");

                    int hi = Convert.ToInt32(horainicio);

                    int hf = Convert.ToInt32(horafin);

                    if (string.Equals(item.idObjeto, Puesto) && (string.Equals(_fechaFinal2, _fechaFinal)))
                    {
                        if ((hi >= hiitems && hi <= hfitems))
                        {
                            reservado = true;
                            fechanoreserva = fechanoreserva + nombredia + " " + _fechainicial + " <br>";
                            continue;
                        }
                        if ((hf >= hfitems && hf <= hfitems))
                        {
                            fechanoreserva = fechanoreserva + nombredia + " " + _fechainicial + " <br>";
                            reservado = true;
                            continue;
                        }
                        if ((hf <= hiitems && hf >= hfitems))
                        {
                            fechanoreserva = fechanoreserva + nombredia + " " + _fechainicial + " <br>";
                            reservado = true;
                            continue;
                        }

                    }
                }

                if (reservado == false)
                {
                    //recorremos los dias
                    if (contador == 25)
                    {
                        // salimos después de cierta cantidad de reservas.
                        break;
                    }

                    id = ids[0];

                    string[] array = id.Split(',');

                    idc = array[0].Count();

                    if (idc > 0)
                    {
                        foreach (var item in misreservasHoraAlmuerzo)
                        {
                            if (item.Fecha == _fechainicial.ToString("yyyy-MM-dd") && item.idHoraAlmuerzo == Almuerzo)
                            {
                                _contador += 1;
                            }
                        }

                        if (_contador > 6)
                        {
                            _horaalmuerzo = "4";
                        }
                        else
                        {
                            _horaalmuerzo = Almuerzo;
                        }

                        //Si se escoge la sala de junta o el auditorio la hora de almuerzo no aplica.
                        if (Puesto == "1221" || Puesto == "1222")
                        {
                            _horaalmuerzo = "1002";
                        }

                        foreach (var item in array)
                        {
                            if (item.ToLower() == nombredia)
                            {
                                Thread.Sleep(500);

                                //Insertamos el Elemento.
                                using (var conexion = new SqlConnection(conexionData))
                                {
                                    //usamos un procedimiento almacenado para insertar la reserva
                                    var procedimiento = "IngresarReserva";

                                    var parametros = new DynamicParameters();
                                    parametros.Add("idObjeto", Puesto);
                                    parametros.Add("Fecha", _fechainicial.ToString("yyyy-MM-dd"));
                                    parametros.Add("HoraInicio", horainicio);
                                    parametros.Add("HoraFin", horafin);
                                    parametros.Add("idUsuario", usuarios);
                                    parametros.Add("idHoraAlmuerzo", _horaalmuerzo);
                                    parametros.Add("observaciones", "");
                                    parametros.Add("id", dbType: System.Data.DbType.String, direction: System.Data.ParameterDirection.Output, size: 50);

                                    conexion.Execute(procedimiento, parametros, commandType: System.Data.CommandType.StoredProcedure);

                                    var _idRespuesta = parametros.Get<object>("id");

                                    if (_idRespuesta.ToString() == "0")
                                    {
                                        fechanoreserva = fechanoreserva + " " + nombredia + " " + _fechainicial.ToString("yyyy-MM-dd") + ", <br>";
                                        entro = true;
                                    }
                                }
                                contador++;
                            }
                        }

                    }

                }

                _fechainicial += new TimeSpan(1, 0, 0, 0);

                reservado = false;
            }

           

            if (entro == true)
            {
                await Helper.EnviarConfirmacion(fechainicial, fechafin, CorreoElectronico, fechanoreserva);
                Helper.Mensaje = "Proceso Finalizado pero hubieron fechas que no se pudo reservar debido a que ya se encuentran reservadas, REVISAR EL CORREO DE CONFIRMACION !";
                Helper.Controller = "/MapaCompleto/Index/";
                return RedirectToAction("Index", "MensajeGenericoWR");
            }
            else
            {
                await Helper.EnviarConfirmacion(fechainicial, fechafin, CorreoElectronico);
                Helper.Mensaje = "Proceso Finalizado exitosamente !";
                Helper.Controller = "/MapaCompleto/Index/";
                return RedirectToAction("Index", "MensajeGenericoOK");
            }

        }

        public ActionResult GoToBack()
        {
            return RedirectToAction("Index", "MapaCompleto");
        }

        public void ConsultaReservas()
        {
            var Consulta = "select tr.idObjeto,NombreCompleto,CONVERT(VARCHAR(12), Fecha, 13) as FechaReserva,HoraInicio,HoraFin,Fecha,tu.idUsuario from tblreservas tr inner join tblObjetos tob on tr.idObjeto = tob.idObjeto inner join tblArea ta on tob.idArea = ta.idArea inner join tblTipoObjeto tto on tob.idTipoObjeto = tto.idTipoObjeto inner join DT_ControlUsuarios.dbo.tblUsuarios tu on tr.idUsuario = tu.idUsuario order by idReserva desc";
            using (var conexion = new SqlConnection(conexionData))
            {
                var Data = conexion.Query<ConsultaReserva>(Consulta).ToList();

                foreach (var item in Data)
                {
                    if (item.Fecha > System.DateTime.Now)
                    {
                        item.Disponible = "No Vencido";
                    }
                    else if (item.Fecha == System.DateTime.Now)
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


        // VGERIFICAMOS QUE USUARIO NO TENGA RESERVA PARA LA FECHA SELECCIONADA.
        public bool TieneReserva(string idUsuario, DateTime Fecha)
        {

            var reservado = reservas.Where(z => z.Fecha == Fecha && z.idUsuario == idUsuario && (z.idObjeto != "1221" || z.idObjeto != "1222")).ToList();

            if (reservado.Count() > 0)
            {
                return true;
            }

            return false;
        }

        //VERIFICAMOS QUE SI EL PUESTO PUESTO TIENE RESERVA PARA UNA FECHA ENVIE CORREO INFORMANDO EL FALLO.
        public bool verificacionhorarioreserva(string idpuesto, DateTime Fechainicial, DateTime fechaFin)
        {

            var reservado = reservas.Where(z => z.Fecha >= Fechainicial && z.Fecha <= fechaFin && z.idObjeto == idpuesto).ToList();

            if (reservado.Count() > 0)
            {
                return true;
            }

            return false;
        }

        //VERIFICAMOS CUANTAS RESERVAS PENDIENTE TIENE EL PUESTO
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