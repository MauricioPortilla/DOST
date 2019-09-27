using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DOST.DataAccess;

namespace DOST.Services {

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class CuentaService : ICuentaService {
        public Cuenta TryLogin(string usuario, string password) {
            string hashedPassword = Engine.HashWithSHA256(password);
            Cuenta cuenta = new Cuenta();
            using (DostDatabase db = new DostDatabase()) {
                var cuentaDb = db.Cuenta.ToList().Find(
                    account => account.usuario == usuario && account.password == hashedPassword
                );
                if (cuentaDb != null) {
                    cuenta.Id = cuentaDb.idcuenta;
                    cuenta.Usuario = cuentaDb.usuario;
                    cuenta.Password = cuentaDb.password;
                    cuenta.Correo = cuentaDb.correo;
                    cuenta.Monedas = cuentaDb.monedas;
                    cuenta.FechaCreacion = cuentaDb.fechaCreacion;
                    cuenta.Confirmada = cuentaDb.confirmada == 1;
                    cuenta.CodigoValidacion = cuentaDb.codigoValidacion;
                    if (cuenta.Confirmada == false) {
                        if (TryValidateAccount(cuenta.CodigoValidacion)) {
                            cuentaDb.confirmada = 1;
                            if (db.SaveChanges() != 0) {
                                cuenta.Confirmada = true;
                            }
                        }
                    }
                }
            }
            return cuenta;
        }

        public bool SignUp(Cuenta cuenta) {
            using (DostDatabase db = new DostDatabase()) {
                var validationCode = Engine.HashWithSHA256(cuenta.Usuario + DateTime.Now);
                db.Cuenta.Add(new DataAccess.Cuenta {
                    usuario = cuenta.Usuario,
                    password = Engine.HashWithSHA256(cuenta.Password),
                    correo = cuenta.Correo,
                    monedas = 0,
                    fechaCreacion = DateTime.Now,
                    confirmada = 0,
                    codigoValidacion = validationCode
                });
                if (db.SaveChanges() != 0) {
                    SendSignUpEmail(cuenta.Correo, validationCode);
                    return true;
                }
                return false;
            }
        }

        private static bool TryValidateAccount(string codigoValidacion) {
            HttpWebResponse response = null;
            string result = string.Empty;
            try {
                HttpWebRequest dostWebRequest = (HttpWebRequest) WebRequest.Create(
                    "https://www.arkanapp.com/dost/dost.php?checkValidationCode=" + codigoValidacion
                );
                dostWebRequest.Method = "GET";
                response = (HttpWebResponse) dostWebRequest.GetResponse();
                using (StreamReader dostStreamReader = new StreamReader(
                    response.GetResponseStream(), Encoding.GetEncoding(response.CharacterSet)
                )) {
                    result = dostStreamReader.ReadToEnd();
                }
            } catch (WebException webException) {
                Console.WriteLine("Error web request: " + webException.Message);
            } finally {
                if (response != null) {
                    response.Close();
                }
            }
            return string.IsNullOrWhiteSpace(result) ? false : ((result == "0") ? false : true);
        }

        private static bool SendSignUpEmail(string email, string codigoValidacion) {
            var xmlElements = Engine.GetConfigFileElements();
            try {
                SmtpClient client = new SmtpClient(xmlElements["Smtp"]["SMTPServer"]);
                var mail = new MailMessage();
                mail.From = new MailAddress(xmlElements["Smtp"]["Email"], "DOST");
                mail.To.Add(email);
                mail.Subject = "Activa tu cuenta en DOST";
                mail.IsBodyHtml = true;
                mail.Body = "<h3>¡Bienvenido a DOST!</h3><br>" +
                    "Da clic <a href=\"https://www.arkanapp.com/dost/dost.php?validationcode=" +
                    codigoValidacion + "\" target=\"_blank\">AQUÍ</a> para activar tu cuenta." +
                    "<br><br>¡Diviértete!<br>-El equipo de DOST";
                if (int.TryParse(xmlElements["Smtp"]["Port"], out int port)) {
                    client.Port = port;
                } else {
                    client.Port = 587;
                }
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(xmlElements["Smtp"]["Email"], xmlElements["Smtp"]["EmailPassword"]);
                client.EnableSsl = true;
                client.Send(mail);
                return true;
            } catch (Exception e) {
                Console.WriteLine("SMTP Exception -> " + e.Message);
                return false;
            }
        }
    }
}
