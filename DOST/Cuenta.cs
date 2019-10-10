using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class Cuenta {
        private int id;
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private string username;
        public string Username {
            get { return username; }
            set { username = value; }
        }
        private string password; // cifrada con SHA1
        public string Password {
            get { return password; }
            set { password = value; }
        }
        private string email;
        public string Email {
            get { return email; }
            set { email = value; }
        }
        private int coins;
        public int Coins {
            get { return coins; }
            set { coins = value; }
        }
        private DateTime creationDate;
        public DateTime CreationDate {
            get { return creationDate; }
            set { creationDate = value; }
        }
        private bool verified;
        public bool Verified {
            get { return verified; }
            set { verified = value; }
        }
        private string validationCode;
        public string ValidationCode {
            get { return validationCode; }
            set { validationCode = value; }
        }

        public Cuenta(int id) {
            this.id = id;
        }

        public Cuenta(string username, string password) {
            this.username = username;
            this.password = password;
        }

        public Cuenta(
            int id, string username, string password, string email, int coins,
            DateTime creationDate, bool verified, string validationCode
        ) {
            this.id = id;
            this.username = username;
            this.password = password;
            this.email = email;
            this.coins = coins;
            this.creationDate = creationDate;
            this.verified = verified;
            this.validationCode = validationCode;
        }

        public bool Login() {
            return EngineNetwork.EstablishChannel<ICuentaService>((loginService) => {
                var account = loginService.TryLogin(username, password);
                id = account.Id;
                verified = account.Confirmada;
                if (account.Id == 0) {
                    return false;
                } else if (!account.Confirmada) {
                    return false;
                }
                username = account.Usuario;
                password = account.Password;
                email = account.Correo;
                coins = account.Monedas;
                creationDate = account.FechaCreacion;
                validationCode = account.CodigoValidacion;
                return true;
            });
        }

        public bool Register() {
            return EngineNetwork.EstablishChannel<ICuentaService>((registerService) => {
                Services.Cuenta account = new Services.Cuenta(
                    0, username, password, email, 0, DateTime.Now, false, null
                );
                return registerService.SignUp(account);
            });
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

        public bool CreateGame(out int idgame) {
            int idNewGame = 0;
            bool returnedValue = EngineNetwork.EstablishChannel<IPartidaService>((service) => {
                return service.CreatePartida(out idNewGame);
            });
            idgame = idNewGame;
            return returnedValue;
        }
    }
}
