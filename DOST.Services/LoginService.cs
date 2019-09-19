using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class LoginService : ILoginService {
        public Cuenta TryLogin(string usuario, string password) {
            Database.InitializeDatabase();
            string hashedPassword = Engine.HashWithSHA256(password);
            Cuenta cuenta = new Cuenta();
            Database.ExecuteStoreQuery(
                "SELECT * FROM cuenta WHERE usuario = @username AND password = @password",
                new Dictionary<string, object>() {
                    { "@username", usuario }, { "@password", hashedPassword }
                }, (results) => {
                    var row = results[0];
                    if ((int) row["confirmado"] == 0) {
                        if (!TryValidateAccount(row["codigoValidacion"].ToString())) {
                            cuenta.Confirmada = false;
                            return;
                        } else {
                            Database.ExecuteUpdate(
                                "UPDATE cuenta SET confirmado = 1 WHERE idcuenta = @idcuenta",
                                new Dictionary<string, object>() {
                                    { "@idcuenta", (int) row["idcuenta"] }
                                }
                            );
                            row["confirmado"] = 1;
                        }
                    }
                    cuenta.Id = (int) row["idcuenta"];
                    cuenta.Usuario = row["usuario"].ToString();
                    cuenta.Password = row["password"].ToString();
                    cuenta.Correo = row["correo"].ToString();
                    cuenta.Monedas = (int) row["monedas"];
                    cuenta.FechaCreacion = DateTime.Parse(row["fechaCreacion"].ToString());
                    cuenta.Confirmada = true;
                    cuenta.CodigoValidacion = row["codigoValidacion"].ToString();
                }
            );
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
