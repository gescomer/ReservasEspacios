using Dapper;
using ReportingREC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ReportingREC
{
    public partial class GetSendInfo : ServiceBase
    {
        bool ejecucion = false;
        bool enviado = false;
        string conexionData = Helper.GetConnection();
        List<Asistente> DataAsistentes;
        List<Reserva> ReservasHoy;

        public GetSendInfo()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            stSendData.Start();
        }

        protected override void OnStop()
        {
            stSendData.Stop();
        }

        private async void stSendData_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<Asistente> Horaentrada = new List<Asistente>();
            List<Asistente> HoraentradaFinal = new List<Asistente>();
            List<Asistente> HoraSalida = new List<Asistente>();
            string nombredia = System.DateTime.Now.ToString("dddd");

            if (ejecucion == true)
            {
                return;
            }

            var fech = System.DateTime.Now.Date.ToString("yyyyMMdd");
            var fechCons = System.DateTime.Now.Date.ToString("yyyy-MM-dd");

            var Consulta2 = "select (tp.emp_code),first_name,last_name,punch_time,passport from zkbiotime..iclock_transaction  tt right join zkbiotime..personnel_employee tp on tt.emp_code = tp.emp_code where punch_time >= '" + fech + "' and emp_type_id=2 order by emp_code";


            var Consulta3 = " select tu.idUsuario,TR.idReserva, Fecha,HoraInicio,HoraFin,Observaciones,Nombre,Codigo,NombreArea,NombreCompleto,Contrasena,Correo from tblReservas tr " +
                            " inner join tblObjetos tob on tr.idObjeto = tob.idObjeto " +
                            " inner join tblArea ta on tob.idArea = ta.idArea " +
                            " inner join DT_ControlUsuarios..tblusuarios tu on tr.idUsuario = tu.idUsuario where Fecha = '" + fechCons + "'";

            using (var conexion = new SqlConnection(conexionData))
            {
                ReservasHoy = conexion.Query<Reserva>(Consulta3).ToList();
            }

            using (var conexion = new SqlConnection(conexionData))
            {
                DataAsistentes = conexion.Query<Asistente>(Consulta2).ToList();

                foreach (var item in DataAsistentes)
                {
                    System.Threading.Thread.Sleep(300);
                    item.passport = Helper.Encryptation(item.passport);
                }
            }

            foreach (var item in DataAsistentes)
            {
                foreach (var item2 in ReservasHoy)
                {
                    item2.Asistio = "Si";
                    item2.Reservo = "No";

                    item.Asistio = "Si";
                    item.Reservo = "No";

                    if (item.passport == item2.Contrasena)
                    {

                        item2.Reservo = "Si";
                        item.Reservo = "Si";

                        break;
                    }
                }
            }

            foreach (var item in ReservasHoy)
            {
                foreach (var item2 in DataAsistentes)
                {
                    var encontro = DataAsistentes.Where(bus => bus.passport == item.Contrasena);

                    if (encontro.Count() > 0)
                    {
                        item.ReservoNoAsistio = "No";
                    }
                }
            }

            string cuerpo = "";
            string Reporte = "";
            string finalizacion = "";
            string Encabezado = "<table style='width: 100%;border:solid 1px green;'><tbody><tr><td style = 'width: 33.3333%;border:solid 1px green;'><div style='text-align: center;'><strong><span style='font-size: 20px; font-family:'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>NOMBRE</span></strong></div></td><td style='width: 33.3333%;border:solid 1px green;'><div style='text-align: center;'><strong><span style='font-size: 20px; font-family: 'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>RESERVO</span></strong></div></td><td style='width: 33.3333%;border:solid 1px green;'><div style='text-align: center;'><strong><span style='font-size: 20px; font-family: 'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>ASISTIO</span></strong></div></td><td style='width: 33.3333%;border:solid 1px green;'><div style='text-align: center;'><strong><span style='font-size: 20px; font-family: 'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>HORA ENTRADA</span></strong></div></td></tr>";
            
            //Eliminador de reservas
            foreach (var item in ReservasHoy)
            {
                if (string.IsNullOrEmpty(item.ReservoNoAsistio))
                {
                    var hora = item.HoraInicio.Substring(0, 2);
                    var minutos = item.HoraInicio.Substring(2, 2);
                    var HoraCon = item.Fecha.ToString("dd/MM/yyyy") + " " + hora + ":" + minutos;


                    var fechahora = Convert.ToDateTime(HoraCon);
                    //agregamos media hora mas
                    fechahora = fechahora.AddMinutes(30);
                    //var Asistio = ReservasHoy.Where<>
                    if (System.DateTime.Now >= fechahora)
                    {
                        Reporte = Reporte + "<tr><td style = 'width: 33.3333%;border:solid 1px blue;'><div style='text-align: center;'><span style='font-size: 20px; font-family:'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>" + item.NombreCompleto + "( " + item.Nombre + " ) </span></div></td><td style = 'width: 33.3333%;border:solid 1px blue;'><div style='text-align: center;'><span style='font-size: 20px; font-family:'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>Si</span></div></td><td style = 'width: 33.3333%;border:solid 1px blue;'><div style='text-align: center;'><span style='font-size: 20px; font-family:'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>No</span></div></td><td style = 'width: 33.3333%;border:solid 1px blue;'><div style='text-align: center;'><span style='font-size: 20px; font-family:'Lucida Sans Unicode', 'Lucida Grande', sans-serif; color: rgb(97, 189, 109);'>" + item.HoraInicio + "-" + item.HoraFin + "</span></div></td></tr>";
                        //cuerpo = cuerpo + " - " + item.NombreCompleto;

                        cuerpo = "<p>La Reserva realizada ha sido eliminada auotmaticamente.</p><p><table style='width: 100%; border-collapse: collapse; border: 1px solid rgb(0, 0, 0); font-family: 'Lucida Console', Monaco, monospace;'><tbody><tr><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>Nombre Completo</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>Puesto</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>Fecha</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>Hora Incio</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>Hora Fin</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>Estado</td></tr><tr><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>" + item.NombreCompleto + "</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>" + item.Nombre + "</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>" + item.Fecha + "</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>" + item.HoraInicio + "</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>" + item.HoraFin + "</td><td style='width: 20%; border: 1px solid rgb(0, 0, 0);'>Eliminado, Inasistencia</td></tr></tbody></table>";

                        string Consulta = "delete from DT_ReservasEspacios..tblReservas where idReserva = " + item.idReserva;

                        using (var conexion = new SqlConnection(conexionData))
                        {
                            var Data = conexion.Query<Reserva>(Consulta).ToList();
                        }

                        await Helper.EnviarConfirmacion(cuerpo, item.Correo,"");

                    }

                }

            }

            finalizacion = "</tbody></table>";

            var fechainicial = System.DateTime.Now.ToString("dd/MM/yyyy") + " 09:00";
            var fechafinal = System.DateTime.Now.ToString("dd/MM/yyyy") + " 09:45";
            string correo = Encabezado + Reporte + finalizacion;

            if (System.DateTime.Now >= Convert.ToDateTime(fechainicial) &&
                System.DateTime.Now <= Convert.ToDateTime(fechafinal) &&
                enviado == false)
            {
                //await Helper.EnviarConfirmacion(correo, "losorio@conhydra.com");
                //enviado = true;
                //Reporte = string.Empty;
                //EventLog.WriteEntry("Correo Hora entrada Enviado a Gestión Humana automaticamente.", EventLogEntryType.Information);

            }
            else
            {
                //enviado = false;
            }


            //Reporte de entrada
            var fechainicialsalida = System.DateTime.Now.ToString("dd/MM/yyyy") + " 9:00";
            var fechafinalsalida = System.DateTime.Now.ToString("dd/MM/yyyy") + " 9:30";

            if (System.DateTime.Now >= Convert.ToDateTime(fechainicialsalida) &&
              System.DateTime.Now <= Convert.ToDateTime(fechafinalsalida) &&
              enviado == false && !string.Equals(nombredia,"sábado") && !string.Equals(nombredia, "domingo"))
            {
                Encabezado = "<table style='width: 100%; border-collapse: collapse; border: 1px solid rgb(0, 0, 0);'><tbody><tr><td style='width: 50%; border: 1px solid rgb(0, 0, 0);background-color:#667fff;text-align: center;'>Nombre Completo</td><td style='width: 50%; border: 1px solid rgb(0, 0, 0);background-color:#667fff;text-align: center;'>Hora llegada</td></tr>";

                cuerpo = string.Empty;

                Consulta3 = "select (tp.emp_code),first_name,last_name,punch_time,passport  " +
                    "from zkbiotime..iclock_transaction  tt right join zkbiotime..personnel_employee tp on tt.emp_code = tp.emp_code " +
                    "where punch_time >= '" + fechCons + "' ORDER BY tp.first_name, punch_time";

                using (var conexion = new SqlConnection(conexionData))
                {
                    DataAsistentes = conexion.Query<Asistente>(Consulta3).ToList();
                }

                foreach (var item in DataAsistentes)
                {
                    var llegada = DataAsistentes.OrderByDescending(x => x.punch_time).Last(x => x.passport == item.passport);
                    Horaentrada.Add(llegada);
                }

                IEnumerable<Asistente> Datos = Horaentrada.Distinct();

                //var cantidades = (from item in Horaentrada orderby item.first_name, item.punch_time select item.first_name).Distinct();
                //HoraentradaFinal =

                foreach (var item in Datos)
                {
                    cuerpo = cuerpo + "<tr><td style='width: 50%; border: 1px solid rgb(0, 0, 0);'>" + item.first_name +
                        "</td><td style='width: 50%; border: 1px solid rgb(0, 0, 0);'>" + item.punch_time + "</td></tr>";
                }

                finalizacion = "</tbody></table><p>Reporte enviado automaticamente. Total Asistencia: " + Datos.Count() + "</p>";

                correo = Encabezado + cuerpo + finalizacion;

                //Enviamos el correo con el reporte de la hora de entrada
                await Helper.EnviarConfirmacion(correo, "losorio@conhydra.com","Si");
                enviado = true;
            }
            else
            {
                enviado = false;
            }


            // reporte de salidas 


            fechainicialsalida = System.DateTime.Now.ToString("dd/MM/yyyy") + " 18:00";
            fechafinalsalida = System.DateTime.Now.ToString("dd/MM/yyyy") + " 18:30";

            if (System.DateTime.Now >= Convert.ToDateTime(fechainicialsalida) &&
              System.DateTime.Now <= Convert.ToDateTime(fechafinalsalida) &&
              enviado == false && !string.Equals(nombredia, "sábado") 
              && !string.Equals(nombredia, "domingo"))
            {

                

                Encabezado = "<table style='width: 100%; border-collapse: collapse; border: 1px solid rgb(0, 0, 0);'><tbody><tr><td style='width: 50%; border: 1px solid rgb(0, 0, 0);'>Nombre Completo</td><td style='width: 50%; border: 1px solid rgb(0, 0, 0);'>Hora Salida</td></tr>";

                cuerpo = string.Empty;

                Consulta3 = "select (tp.emp_code),first_name,last_name,punch_time,passport  " +
                    "from zkbiotime..iclock_transaction  tt right join zkbiotime..personnel_employee tp on tt.emp_code = tp.emp_code " +
                    "where punch_time >= '" + fechCons + "' ORDER BY tp.first_name, punch_time";

                using (var conexion = new SqlConnection(conexionData))
                {
                    DataAsistentes = conexion.Query<Asistente>(Consulta3).ToList();
                }

                foreach (var item in DataAsistentes)
                {
                    var llegada = DataAsistentes.OrderByDescending(x => x.punch_time).First(x => x.passport == item.passport);
                    var salida = DataAsistentes.OrderBy(x => x.punch_time).First(x => x.passport == item.passport);
                    Horaentrada.Add(llegada);
                }

                IEnumerable<Asistente> DatosSalida = Horaentrada.Distinct();

                foreach (var item in DatosSalida)
                {
                    cuerpo = cuerpo + "<tr><td style='width: 50%; border: 1px solid rgb(0, 0, 0);'>" + item.first_name +
                        "</td><td style='width: 50%; border: 1px solid rgb(0, 0, 0);'>" + item.punch_time + "</td></tr>";
                }

                finalizacion = "</tbody></table><p>Reporte enviado automaticamente.</p>";

                correo = Encabezado + cuerpo + finalizacion;


                //Enviamos el correo con el reporte de la hora de entrada
                await Helper.EnviarConfirmacion(correo, "losorio@conhydra.com","Si");
                enviado = true;
            }
            else
            {
                enviado = false;
            }

            ejecucion = false;

        }
    }
}
