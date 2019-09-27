using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class Cuenta {
        private int id;
        public int Id { get { return id; } set { id = value; } }
        private string usuario;
        public string Usuario { get { return usuario; } set { usuario = value; } }
        private string password; // cifrada con SHA1
        public string Password { get { return password; } set { password = value; } }
        private string correo;
        public string Correo { get { return correo; } set { correo = value; } }
        private int monedas;
        public int Monedas { get { return monedas; } set { monedas = value; } }
        private DateTime fechaCreacion;
        public DateTime FechaCreacion {
            get { return fechaCreacion; }
            set { fechaCreacion = value; }
        }
        private bool confirmada;
        public bool Confirmada {
            get { return confirmada; }
            set { confirmada = value; }
        }
        private string codigoValidacion;
        public string CodigoValidacion {
            get { return codigoValidacion; }
            set { codigoValidacion = value; }
        }

        public Cuenta(int id) {
            this.id = id;
            /*EngineNetwork.Send(EngineNetwork.CreatePackage(new object[] {
                (byte) NetworkClientRequests.GetAccountData, id
            }));
            var package = EngineNetwork.ReceiveMultipleData();
            if (package.Count < 1) {
                return;
            }
            if (!package[0].ContainsKey("code")) {
                return;
            } else if (byte.Parse(package[0]["code"]) != (byte) NetworkServerResponses.AccountData) {
                return;
            }
            package.RemoveAll(x => x.ContainsKey("code"));
            var accountData = package[0];
            if (accountData.Count > 0) {
                id = int.Parse(accountData["idcuenta"]);
                usuario = accountData["usuario"];
                password = accountData["password"];
                correo = accountData["correo"];
                monedas = int.Parse(accountData["monedas"]);
                fechaCreacion = DateTime.Parse(accountData["fechaCreacion"]);
                confirmada = (accountData["confirmado"] == "1") ? true : false;
                codigoValidacion = accountData["codigoValidacion"];
            }*/
        }

        public Cuenta(string usuario, string password) {
            this.usuario = usuario;
            this.password = password;
        }

        public Cuenta(
            int id, string usuario, string password, string correo, int monedas,
            DateTime fechaCreacion, bool confirmada, string codigoValidacion
        ) {
            this.id = id;
            this.usuario = usuario;
            this.password = password;
            this.correo = correo;
            this.monedas = monedas;
            this.fechaCreacion = fechaCreacion;
            this.confirmada = confirmada;
            this.codigoValidacion = codigoValidacion;
        }

        public bool Login() {
            return EngineNetwork.EstablishChannel<ICuentaService>((loginService) => {
                var cuenta = loginService.TryLogin(usuario, password);
                if (cuenta.Id == 0) {
                    return false;
                } else if (!cuenta.Confirmada) {
                    return false;
                }
                id = cuenta.Id;
                usuario = cuenta.Usuario;
                password = cuenta.Password;
                correo = cuenta.Correo;
                monedas = cuenta.Monedas;
                fechaCreacion = cuenta.FechaCreacion;
                confirmada = cuenta.Confirmada;
                codigoValidacion = cuenta.CodigoValidacion;
                return true;
            });
        }

        public bool Register() {
            return EngineNetwork.EstablishChannel<ICuentaService>((registerService) => {
                Services.Cuenta cuenta = new Services.Cuenta(
                    0, usuario, password, correo, 0, DateTime.Now, false, null
                );
                return registerService.SignUp(cuenta);
            });
        }

        public void Logout() {

        }

        public bool JoinGame(Partida game, bool asAnfitrion) {
            return EngineNetwork.EstablishChannel<IPartidaService>((service) => {
                return service.AddJugador(id, game.Id, asAnfitrion);
            });
        }

        public bool LeaveGame(Partida game) {
            return EngineNetwork.EstablishChannel<IPartidaService>((service) => {
                return service.RemoveJugador(id, game.Id);
            });
        }

        public bool CreateGame(out int idpartida) {
            int idgame = 0;
            bool returnedValue = EngineNetwork.EstablishChannel<IPartidaService>((service) => {
                return service.CreatePartida(out idgame);
            });
            idpartida = idgame;
            return returnedValue;
        }
    }
}
