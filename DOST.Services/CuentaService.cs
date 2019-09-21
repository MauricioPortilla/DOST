using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
                }
            }
            return cuenta;
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
    }
}
