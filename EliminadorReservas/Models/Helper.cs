using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EliminadorReservas.Models
{
    class Helper
    {
        public static string GetConnection()
        {
            return ConfigurationManager.ConnectionStrings["conexionBD"].ConnectionString;
        }

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

        public static async Task EnviarConfirmacion(string Cuerpo, string Correo)
        {
            var Mensaje = new MailMessage();
            //Mensaje.To.Add(new MailAddress(""));
            Mensaje.To.Add(new MailAddress("erenteria@conhydra.com"));
            Mensaje.To.Add(new MailAddress(Correo));
            Mensaje.From = new MailAddress("Supervisorchado@gmail.com");
            Mensaje.Subject = "Reporte Ausentismo Conhydra SAS ESP";
            Mensaje.Body = Cuerpo;
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
