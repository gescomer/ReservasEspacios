using AlelaProject.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AlelaProject.Servicio
{
    public class Helper
    {
        public static string Coneccion = ConfigurationManager.ConnectionStrings["mipuesto"].ConnectionString;
        public static string ConeccionUser = ConfigurationManager.ConnectionStrings["login"].ConnectionString;

        public static string Mensaje;
        public static string Controller;

        public static string Encryptation(string pwd)
        {
            SHA256 sHA256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sHA256.ComputeHash(encoding.GetBytes(pwd));
            for (int i = 0; i < stream.Length; i++)
            {
                sb.AppendFormat("{0:x2}", stream[i]);
            }

            return sb.ToString();
        }
        public static string TipoOracion(string str)
        {
            string[] PalabraDivida = str.Split(' ');
            string PalabraFinal = string.Empty;
            foreach (var item in PalabraDivida)
            {
                StringBuilder sb = new StringBuilder(item);
                if (!string.IsNullOrEmpty(str))
                {
                    for (int i = 0; i < sb.Length; i++)
                    {
                        if (i == 0)
                        {

                            sb[i] = char.ToUpper(sb[i]);
                        }
                        else
                        {
                            sb[i] = char.ToLower(sb[i]);
                        }
                    }
                }
                PalabraFinal = PalabraFinal + sb.ToString() + " ";


            }
            return PalabraFinal.Trim();
        }

        public static async Task EnviarConfirmacion(string fechaInicio, string Fechafin, string CorreoElectronico)
        {
            var Mensaje = new MailMessage();
            Mensaje.To.Add(new MailAddress(CorreoElectronico));
            Mensaje.From = new MailAddress("Supervisorchado@gmail.com");
            Mensaje.Subject = "Confirmacion Reserva Espacio Corporativo Conhydra S.A.S E.S.P";
            Mensaje.Body = "Su Asignación de horario de trabajo desde el " + fechaInicio + " Hasta el " + Fechafin + " fue creado exitosamente. Verificar en mis reservas y cumplir con el aforo maximo en el tiempo de almuerzo !";
            Mensaje.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credencial = new NetworkCredential
                {
                    UserName = "Supervisorchado@gmail.com",
                    Password = "mrprswfgmffplgqv",
                };

                smtp.Credentials = credencial;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(Mensaje);

            }
        }
        public static async Task EnviarConfirmacion(string horaInicio, string HoraFin, string Fecha, string Nombre, string CorreoElectronico, string Observaciones)
        {
            var Mensaje = new MailMessage();
            Mensaje.To.Add(new MailAddress(CorreoElectronico));
            Mensaje.From = new MailAddress("Supervisorchado@gmail.com");
            Mensaje.Subject = "Confirmacion Reserva Espacio Corporativo Conhydra S.A.S E.S.P";
            Mensaje.Body = "<table style = 'width: 100%;border:solid 2px'><tbody><tr style='width:100%; border:solid 2px'><td style = 'width: 20%;'><div style ='text-align:center;'><strong><span style ='font-family: Arial, Helvetica, sans-serif;'> PUESTO </span></strong></div></td><td style = 'width: 20%;'><div style = 'text-align:center;'><span style ='font-family: Arial, Helvetica, sans-serif;'><strong> FECHA </strong></span></div></td><td style = 'width: 20%;'><div style = 'text-align: center;'><strong><span style = 'font-family: Arial, Helvetica, sans-serif;'> HORA INICIO </span></strong></div></td><td style = 'width: 20%;'><div style ='text-align: center;'><span style ='font-family: Arial, Helvetica, sans-serif;'><strong> HORA FIN </strong></span></div></td><td style = 'width: 20%;'><div style ='text-align: center;'><span style ='font-family: Arial, Helvetica, sans-serif;'><strong> Observaciones </strong></span></div></td></tr>" +
                "<tr><td style = 'width: 20%;'><div data-empty = 'true' style ='text-align:center;'><span style ='font-family: Courier New, courier;'><strong><span style ='color: rgb(184, 49, 47);'>" + Nombre + "</span></strong></span></div></td><td style ='width: 20%;'><div data-empty = 'true' style = 'text-align:center;'><span style = 'color: rgb(184, 49, 47);'> " + Fecha + " </span></div></td><td style = 'width: 20%;'><div data-empty = 'true' style = 'text-align: center;'><span style = 'color:rgb(184, 49, 47);'> " + horaInicio + " </span></div></td><td style = 'width: 20%;'><div data-empty = 'true' style = 'text-align: center;'><span style = 'color: rgb(184, 49, 47);'>" + HoraFin + "</span></div></td><td style = 'width: 20%;'><div data-empty = 'true' style = 'text-align: center;'><span style = 'color: rgb(184, 49, 47);'>" + Observaciones + "</span></div></td></tr></tbody></table>" +
                "<p><span style='font-size: 20px; font-family: 'Courier New', courier; color: rgb(97, 189, 109);'>Reserva creada exitosamente.</span></p>";
            Mensaje.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credencial = new NetworkCredential
                {
                    UserName = "Supervisorchado@gmail.com",
                    Password = "mrprswfgmffplgqv",
                };

                smtp.Credentials = credencial;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(Mensaje);

            }
        }
        public static async Task EnviarConfirmacion(string fechaInicio, string Fechafin, string CorreoElectronico, string Observaciones)
        {
            var Mensaje = new MailMessage();
            Mensaje.To.Add(new MailAddress(CorreoElectronico));
            Mensaje.From = new MailAddress("Supervisorchado@gmail.com");
            Mensaje.Subject = "Confirmacion Reserva Espacio Corporativo Conhydra S.A.S E.S.P";
            Mensaje.Body = "Su Asignación de horario de trabajo desde el " + fechaInicio + " Hasta el " + Fechafin + " fue creado pero hubieron fechas en la cuales ya se encuentran reservadas.<span style= 'color:red;font-size:19px;font-weight:bold'> Verificar en Mis Reservas si no tienes reservas y reservar  en otro puesto las siguientes fechas:</span> <br><br>" + Observaciones + " <br> Además se debe cumplir con el aforo maximo en el tiempo de almuerzo.";
            Mensaje.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credencial = new NetworkCredential
                {
                    UserName = "Supervisorchado@gmail.com",
                    Password = "mrprswfgmffplgqv",
                };

                smtp.Credentials = credencial;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(Mensaje);

            }
        }
        public static async Task EliminarConfirmacion(string fechaInicio, string CorreoElectronico, string NombrePuesto)
        {
            var Mensaje = new MailMessage();
            Mensaje.To.Add(new MailAddress(CorreoElectronico));
            Mensaje.From = new MailAddress("Supervisorchado@gmail.com");
            Mensaje.Subject = "Confirmacion Reserva Espacio Corporativo Conhydra S.A.S E.S.P";
            Mensaje.Body = "<table style = 'width: 100%;border:solid 2px'><tbody><tr style='width:100%; border:solid 2px'><td style = 'width: 25.0000%;'><div style ='text-align:center;'><strong><span style ='font-family: Arial, Helvetica, sans-serif;'> PUESTO </span></strong></div></td><td style = 'width: 25.0000%;'><div style = 'text-align:center;'><span style ='font-family: Arial, Helvetica, sans-serif;'><strong> FECHA </strong></span></div></td></tr>" +
                "<tr><td style = 'width: 25.0000%;'><div data-empty = 'true' style ='text-align:center;'><span style ='font-family: Courier New, courier;'><strong><span style ='color: rgb(184, 49, 47);'>" + NombrePuesto + "</span></strong></span></div></td><td style ='width: 25.0000%;'><div data-empty = 'true' style = 'text-align:center;'><span style = 'color: rgb(184, 49, 47);'> " + fechaInicio + " </span></div></td></tr></tbody></table>" +
                "<p><span style='font-size: 20px; font-family: 'Courier New', courier; color: rgb(97, 189, 109);'>Reserva eliminada exitosamente.</span></p>";
            Mensaje.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credencial = new NetworkCredential
                {
                    UserName = "Supervisorchado@gmail.com",
                    Password = "mrprswfgmffplgqv",
                };

                smtp.Credentials = credencial;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(Mensaje);

            }
        }
        public static async Task EnviarConfirmacionAlmuerzo(string fechaInicio, string Horaalmuerzo, string CorreoElectronico)
        {
            var Mensaje = new MailMessage();
            Mensaje.To.Add(new MailAddress(CorreoElectronico));
            Mensaje.From = new MailAddress("Supervisorchado@gmail.com");
            Mensaje.Subject = "Confirmacion Reserva Espacio Corporativo Conhydra S.A.S E.S.P";
            Mensaje.Body = "El horario de almuerzo fue establecido con exito. <br> Fecha: " + fechaInicio + "<br> Hora Inicio: " + Horaalmuerzo + "<br> Recuerde cumplir con el aforo máximo por cada hora.";
            Mensaje.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credencial = new NetworkCredential
                {
                    UserName = "Supervisorchado@gmail.com",
                    Password = "mrprswfgmffplgqv",
                };

                smtp.Credentials = credencial;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(Mensaje);

            }
        }
        public static async Task ErrorReserva(string FechaInicio, string FechaFin, string Nombre, string CorreoElectronico)
        {
            var Mensaje = new MailMessage();
            Mensaje.To.Add(new MailAddress(CorreoElectronico));
            Mensaje.From = new MailAddress("Supervisorchado@gmail.com");
            Mensaje.Subject = "Confirmacion Reserva Espacio Corporativo Conhydra S.A.S E.S.P";
            Mensaje.Body = "<span style= 'color:red;font-size:19px;font-weight:bold'>La reserva que deseas realizar en el " + Nombre + " desde " + FechaInicio + " hasta " + FechaFin + " no se pudo realizar debido a que hay algunas fechas que ya tienen reserva en ese puesto. Verficar en la reserva del puestos y reservar en fechas diferentes.</span> <br><br> <br> Además se debe cumplir con el aforo maximo en el tiempo de almuerzo.<br> Muchas gracias.";
            Mensaje.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credencial = new NetworkCredential
                {
                    UserName = "Supervisorchado@gmail.com",
                    Password = "mrprswfgmffplgqv",
                };

                smtp.Credentials = credencial;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(Mensaje);

            }
        }

    }
}