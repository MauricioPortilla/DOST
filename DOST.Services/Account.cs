using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DOST.DataAccess;

namespace DOST.Services {
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

        public Account() {
        }

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

        public Account(int id) {
            this.id = id;
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
        }
    }
}
