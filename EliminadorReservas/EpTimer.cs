using EliminadorReservas.Models;
using System;
using Dapper;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;


namespace EliminadorReservas
{
    public partial class EpTimer : ServiceBase
    {
        bool enviado = false;
        string conexionData = Helper.GetConnection();
        List<Asistente> DataAsistentes;
        List<Reserva> ReservasHoy;
        public EpTimer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            tmInit.Interval = 300000;
            tmInit.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            tmInit.Enabled = true;
            tmInit.Start();

        }

        protected override void OnStop()
        {
            tmInit.Stop();
        }

        private async void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                tmInit.Enabled = false;

                //EventLog.WriteEntry("Inicio el proceso", EventLogEntryType.Information);

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

                            await Helper.EnviarConfirmacion(cuerpo, item.Correo);

                        }

                    }

                }

                finalizacion = "</tbody></table>";

                //EventLog.WriteEntry("Fin el proceso de verificacion de puestos", EventLogEntryType.Information);
                var fechainicial = System.DateTime.Now.ToString("dd/MM/yyyy") + "09:30";
                var fechafinal = System.DateTime.Now.ToString("dd/MM/yyyy") + "10:00";
                string correo = Encabezado + Reporte + finalizacion;

                if (System.DateTime.Now >= Convert.ToDateTime(fechainicial) &&
                    System.DateTime.Now <= Convert.ToDateTime(fechafinal) &&
                    enviado == false)
                {
                    await Helper.EnviarConfirmacion(correo, "losorio@conhydra.com");
                    enviado = true;
                    //EventLog.WriteEntry("Correo Enviado a Gestión Humana.    ", EventLogEntryType.Information);

                }
                else
                {
                    enviado = false;
                }

                tmInit.Enabled = true;

            }
            catch (Exception ex)
            {

                EventLog.WriteEntry("Error aplicacion Eliminador de reservas:  ", ex.Message);
            }

        }
    }
}