using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class Account {
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
        private bool isVerified;
        public bool IsVerified {
            get { return isVerified; }
            set { isVerified = value; }
        }
        private string validationCode;
        public string ValidationCode {
            get { return validationCode; }
            set { validationCode = value; }
        }

        public Account(int id) {
            this.id = id;
        }

        public Account(string username, string password) {
            this.username = username;
            this.password = password;
        }

        public Account(
            int id, string username, string password, string email, int coins,
            DateTime creationDate, bool verified, string validationCode
        ) {
            this.id = id;
            this.username = username;
            this.password = password;
            this.email = email;
            this.coins = coins;
            this.creationDate = creationDate;
            this.isVerified = verified;
            this.validationCode = validationCode;
        }

        public bool Login() {
            return EngineNetwork.EstablishChannel<IAccountService>((loginService) => {
                var account = loginService.TryLogin(username, password);
                id = account.Id;
                isVerified = account.IsVerified;
                if (account.Id == 0) {
                    return false;
                } else if (!account.IsVerified) {
                    return false;
                }
                username = account.Username;
                password = account.Password;
                email = account.Email;
                coins = account.Coins;
                creationDate = account.CreationDate;
                validationCode = account.ValidationCode;
                return true;
            });
        }

        public bool Register() {
            return EngineNetwork.EstablishChannel<IAccountService>((registerService) => {
                Services.Account account = new Services.Account(
                    0, username, password, email, 0, DateTime.Now, false, null
                );
                return registerService.SignUp(account);
            });
        }

        public bool JoinGame(Game game, bool asAnfitrion) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.AddPlayer(id, game.ActiveGuidGame, asAnfitrion);
            });
        }

        public bool CreateGame(out string guidGame) {
            string guidNewGame = "";
            bool returnedValue = EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.CreateGame(out guidNewGame);
            });
            guidGame = guidNewGame;
            return returnedValue;
        }

        public string GetRank() {
            var rank = Properties.Resources.NotRankedText;
            EngineNetwork.EstablishChannel<IAccountService>((service) => {
                var accountRank = service.GetRank(id);
                rank = string.IsNullOrEmpty(accountRank) ? rank : accountRank;
                return true;
            });
            return rank;
        }
    }
}
