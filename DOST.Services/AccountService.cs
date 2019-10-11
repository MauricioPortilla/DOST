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
    public class AccountService : IAccountService {
        public Account TryLogin(string username, string password) {
            string hashedPassword = Engine.HashWithSHA256(password);
            Account account = new Account();
            using (DostDatabase db = new DostDatabase()) {
                var accountDb = db.Account.ToList().Find(
                    accountInList => accountInList.username == username && accountInList.password == hashedPassword
                );
                if (accountDb != null) {
                    account.Id = accountDb.idaccount;
                    account.Username = accountDb.username;
                    account.Password = accountDb.password;
                    account.Email = accountDb.email;
                    account.Coins = accountDb.coins;
                    account.CreationDate = accountDb.creationDate;
                    account.IsVerified = accountDb.isVerified == 1;
                    account.ValidationCode = accountDb.validationCode;
                    if (account.IsVerified == false) {
                        if (TryValidateAccount(account.ValidationCode)) {
                            accountDb.isVerified = 1;
                            if (db.SaveChanges() != 0) {
                                account.IsVerified = true;
                            }
                        }
                    }
                }
            }
            return account;
        }

        public bool SignUp(Account account) {
            using (DostDatabase db = new DostDatabase()) {
                var validationCode = Engine.HashWithSHA256(account.Username + DateTime.Now);
                db.Account.Add(new DataAccess.Account {
                    username = account.Username,
                    password = Engine.HashWithSHA256(account.Password),
                    email = account.Email,
                    coins = 0,
                    creationDate = DateTime.Now,
                    isVerified = 0,
                    validationCode = validationCode
                });
                if (db.SaveChanges() != 0) {
                    SendSignUpEmail(account.Email, validationCode);
                    return true;
                }
                return false;
            }
        }

        private static bool TryValidateAccount(string validationCode) {
            HttpWebResponse response = null;
            string result = string.Empty;
            try {
                HttpWebRequest dostWebRequest = (HttpWebRequest) WebRequest.Create(
                    "https://www.arkanapp.com/dost/dost.php?checkValidationCode=" + validationCode
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

        public List<UserScore> GetBestScores() {
            List<UserScore> bestScoresList = new List<UserScore>();
            using (DostDatabase db = new DostDatabase()) {
                var players = (from player in db.Player
                               where player.Game.round == 5
                               group player by player.Account.username into playerGroup
                               let totalScore = playerGroup.Sum(playerInGroup => playerInGroup.score)
                               orderby totalScore descending
                               select new { User = playerGroup.Key, TotalScore = totalScore }).ToList();
                for (int pos = 0; pos < players.Count; pos++) {
                    bestScoresList.Add(new UserScore {
                        Ranking = pos + 1,
                        Username = players[pos].User,
                        Score = players[pos].TotalScore
                    });
                }
            }
            return bestScoresList;
        }

        public string GetRank(int idaccount) {
            string rank = "No clasificado";
            using (DostDatabase db = new DostDatabase()) {
                var username = db.Account.ToList().Find(cuenta => cuenta.idaccount == idaccount).username;
                if (username != null) {
                    var player = GetBestScores().Find(userScore => userScore.Username == username);
                    if (player != null) {
                        rank = "#" + player.Ranking;
                    }

                }
            }
            return rank;
        }
    }
}
