using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;
using System.Text;
using DOST.DataAccess;

namespace DOST.Services {
    /// <summary>
    /// Manages account operations through network.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class AccountService : IAccountService {
        /// <summary>
        /// Establishes a connection with database to try to find an account with username and password given.
        /// If exists and is not verified, tries to verify it with webpage.
        /// </summary>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        /// <returns>Account found in database</returns>
        public Account TryLogin(string username, string password) {
            string hashedPassword = Engine.HashWithSHA256(password);
            Account account = new Account();
            try {
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
            } catch (DbUpdateException dbUpdateException) {
                Console.WriteLine("DbUpdateException -> " + dbUpdateException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return account;
        }

        /// <summary>
        /// Establishes a connection with database to register a new account.
        /// </summary>
        /// <param name="account">Account to register</param>
        /// <returns>True if account was registered successfully; False if not</returns>
        public bool SignUp(Account account) {
            try {
                using (DostDatabase db = new DostDatabase()) {
                    if (db.Account.ToList().Find(accountRegistered => accountRegistered.email == account.Email) != null) {
                        return false;
                    }
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
                }
            } catch (DbUpdateException dbUpdateException) {
                Console.WriteLine("DbUpdateException -> " + dbUpdateException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return false;
        }

        /// <summary>
        /// Sends a request to webpage to try to validate an account.
        /// </summary>
        /// <param name="validationCode">Account validation code</param>
        /// <returns>True if webpage database has validation code stored; False if not</returns>
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
            } catch (IOException ioException) {
                Console.WriteLine("IOException -> " + ioException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            } finally {
                if (response != null) {
                    response.Close();
                }
            }
            return string.IsNullOrWhiteSpace(result) ? false : ((result == "0") ? false : true);
        }

        /// <summary>
        /// Sends an email to verify the account recently registered.
        /// </summary>
        /// <param name="email">Account email</param>
        /// <param name="validationCode">Account validation code</param>
        /// <returns>True if email was sent successfully; False if not</returns>
        private static bool SendSignUpEmail(string email, string validationCode) {
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
                    validationCode + "\" target=\"_blank\">AQUÍ</a> para activar tu cuenta." +
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
            } catch (InvalidOperationException invalidOperationException) {
                Console.WriteLine("Invalid operation exception -> " + invalidOperationException.Message);
            } catch (SmtpFailedRecipientsException smtpFRException) {
                Console.WriteLine("SMTP failed recipients exception -> " + smtpFRException.Message);
            } catch (SmtpException smtpException) {
                Console.WriteLine("SMTP Exception -> " + smtpException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return false;
        }

        /// <summary>
        /// Establishes a connection with database to try to get all the best scores.
        /// </summary>
        /// <returns>List with best scores</returns>
        public List<UserScore> GetBestScores() {
            List<UserScore> bestScoresList = new List<UserScore>();
            try {
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
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return bestScoresList;
        }

        /// <summary>
        /// Establishes a connection with database to try to get the account rank by identifier given.
        /// </summary>
        /// <param name="idaccount">Account identifier</param>
        /// <returns>Rank with prefix #, or an empty string if couldn't find the account or games played.</returns>
        public string GetRank(int idaccount) {
            string rank = "";
            try {
                using (DostDatabase db = new DostDatabase()) {
                    var account = db.Account.ToList().Find(accountList => accountList.idaccount == idaccount);
                    if (account != null) {
                        var player = GetBestScores().Find(userScore => userScore.Username == account.username);
                        if (player != null) {
                            rank = "#" + player.Ranking;
                        }

                    }
                }
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return rank;
        }

        /// <summary>
        /// Establishes a connection with database to try to get account data by identifier given.
        /// </summary>
        /// <param name="idaccount">Account identifier</param>
        /// <returns>Account found</returns>
        public Account GetAccount(int idaccount) {
            Account account = new Account();
            try {
                using (DostDatabase db = new DostDatabase()) {
                    var accountDb = db.Account.Find(idaccount);
                    if (accountDb != null) {
                        account.Id = accountDb.idaccount;
                        account.Username = accountDb.username;
                        account.Password = accountDb.password;
                        account.Email = accountDb.email;
                        account.Coins = accountDb.coins;
                        account.CreationDate = accountDb.creationDate;
                        account.IsVerified = accountDb.isVerified == 1;
                        account.ValidationCode = accountDb.validationCode;
                    }
                }
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return account;
        }
    }
}
