using System;
using System.Linq;
using System.Runtime.Serialization;
using DOST.DataAccess;

namespace DOST.Services {
    /// <summary>
    /// Represents an account in the service.
    /// </summary>
    [DataContract]
    public class Account {
        private int id;
        [DataMember]
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private string username;
        [DataMember]
        public string Username {
            get { return username; }
            set { username = value; }
        }
        private string password; // cifrada con SHA1
        [DataMember]
        public string Password {
            get { return password; }
            set { password = value; }
        }
        private string email;
        [DataMember]
        public string Email {
            get { return email; }
            set { email = value; }
        }
        private int coins;
        [DataMember]
        public int Coins {
            get { return coins; }
            set { coins = value; }
        }
        private DateTime creationDate;
        [DataMember]
        public DateTime CreationDate {
            get { return creationDate; }
            set { creationDate = value; }
        }
        private bool isVerified;
        [DataMember]
        public bool IsVerified {
            get { return isVerified; }
            set { isVerified = value; }
        }
        private string validationCode;
        [DataMember]
        public string ValidationCode {
            get { return validationCode; }
            set { validationCode = value; }
        }

        /// <summary>
        /// Creates and initializes a new empty instance.
        /// </summary>
        public Account() {
        }

        /// <summary>
        /// Creates and initializes a new instance with data given.
        /// </summary>
        /// <param name="id">Account identifier</param>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        /// <param name="email">Account email</param>
        /// <param name="coins">Account coins</param>
        /// <param name="creationDate">Account creation date</param>
        /// <param name="isVerified">True if account is verified; False if not</param>
        /// <param name="validationCode">Account validation code</param>
        public Account(
            int id, string username, string password, string email, int coins,
            DateTime creationDate, bool isVerified, string validationCode
        ) {
            this.id = id;
            this.username = username;
            this.password = password;
            this.email = email;
            this.coins = coins;
            this.creationDate = creationDate;
            this.isVerified = isVerified;
            this.validationCode = validationCode;
        }

        /// <summary>
        /// Creates and initializes a new instance with identifier given and establishes a connection with
        /// database to try to load the account data.
        /// </summary>
        /// <param name="id">Account identifier</param>
        public Account(int id) {
            this.id = id;
            try {
                using (DostDatabase db = new DostDatabase()) {
                    var accountDb = db.Account.ToList().Find(account => account.idaccount == id);
                    if (accountDb != null) {
                        username = accountDb.username;
                        password = accountDb.password;
                        email = accountDb.email;
                        coins = accountDb.coins;
                        creationDate = accountDb.creationDate;
                        isVerified = accountDb.isVerified == 1;
                        validationCode = accountDb.validationCode;
                    }
                }
            } catch (Exception exception) {
                Console.WriteLine("Exception (Account) -> " + exception.Message);
            }
        }
    }
}
