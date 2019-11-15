using DOST.Services;
using System;

namespace DOST {
    /// <summary>
    /// Represents an account in the game.
    /// </summary>
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

        /// <summary>
        /// Creates an account instance based on an account identifier.
        /// </summary>
        /// <param name="id">Account identifier</param>
        public Account(int id) {
            this.id = id;
        }

        /// <summary>
        /// Creates an account instance based on an username and a password.
        /// </summary>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        public Account(string username, string password) {
            this.username = username;
            this.password = password;
        }

        /// <summary>
        /// Creates an account instance given complete data.
        /// </summary>
        /// <param name="id">Account identifier</param>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        /// <param name="email">Account email</param>
        /// <param name="coins">Account coins</param>
        /// <param name="creationDate">Account creation date</param>
        /// <param name="verified">Is the account verified</param>
        /// <param name="validationCode">Account validation code</param>
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

        /// <summary>
        /// Establishes a connection with account service and checks if there's an account registered
        /// with the username and password associated to this account.
        /// </summary>
        /// <returns>True if exists; False if not exists</returns>
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

        /// <summary>
        /// Establishes a connection with account service to try to register an account
        /// with the data stored in this account instance.
        /// </summary>
        /// <returns>True if account was registered successfully; False if not</returns>
        public bool Register() {
            return EngineNetwork.EstablishChannel<IAccountService>((registerService) => {
                Services.Account account = new Services.Account(
                    0, username, password, email, 0, DateTime.Now, false, null
                );
                return registerService.SignUp(account);
            });
        }

        /// <summary>
        /// Establishes a connection with account service to try to reload account data.
        /// </summary>
        /// <returns>True if account data was reloaded successfully; False if not</returns>
        public bool Reload() {
            return EngineNetwork.EstablishChannel<IAccountService>((loginService) => {
                var account = loginService.GetAccount(id);
                if (account.Id == 0) {
                    return false;
                }
                username = account.Username;
                password = account.Password;
                email = account.Email;
                coins = account.Coins;
                creationDate = account.CreationDate;
                isVerified = account.IsVerified;
                validationCode = account.ValidationCode;
                return true;
            });
        }

        /// <summary>
        /// Establishes a connnection with game service to try to join to a game.
        /// </summary>
        /// <param name="game">Game to join in</param>
        /// <param name="asAnfitrion">True to join as host; False to join as guest</param>
        /// <param name="guidPlayer">Player global unique identifier generated</param>
        /// <returns>True if join request was successful; False if not</returns>
        public bool JoinGame(Game game, bool asAnfitrion, out string guidPlayer) {
            string guidNewPlayer = "";
            bool returnedValue = EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.AddPlayer(id, game.ActiveGuidGame, asAnfitrion, out guidNewPlayer);
            });
            guidPlayer = guidNewPlayer;
            return returnedValue;
        }

        /// <summary>
        /// Establishes a connnection with game service to try to create a new game.
        /// </summary>
        /// <param name="guidGame">
        ///     Stores a global unique identifier that identifies the new game created.
        ///     If game couldn't be created, this value will be empty.
        /// </param>
        /// <returns>True if creation request was successful; False if not</returns>
        public bool CreateGame(out string guidGame) {
            string guidNewGame = "";
            bool returnedValue = EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.CreateGame(out guidNewGame, App.Language);
            });
            guidGame = guidNewGame;
            return returnedValue;
        }

        /// <summary>
        /// Establishes a connnection with account service to get the account rank.
        /// </summary>
        /// <returns>
        ///     If this account has games played, will return rank as #N, where N is the place.
        ///     If it has no games played, will return a "Not ranked" string.
        /// </returns>
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
