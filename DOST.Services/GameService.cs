using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DOST.DataAccess;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class GameService : IGameService {
        /// <summary>
        /// Stores active games ID with players lists.
        /// </summary>
        private static readonly List<Game> activeGames = new List<Game>();
        private static readonly int MAX_PLAYERS_IN_GAME = 4;

        public List<Game> GetGamesList() {
            return activeGames;
        }

        public List<Player> GetPlayersList(int idgame) {
            List<Player> playersList = new List<Player>();
            try {
                using (DostDatabase db = new DostDatabase()) {
                    var playersListDb = (from player in db.Player
                                         where player.idgame == idgame
                                         select player).ToList();
                    playersListDb.ForEach(
                        player => playersList.Add(new Player(
                            player.idplayer,
                            new Account(
                                player.Account.idaccount, player.Account.username,
                                player.Account.password, player.Account.email,
                                player.Account.coins, player.Account.creationDate,
                                player.Account.isVerified == 1, player.Account.validationCode
                            ),
                            null,
                            player.score,
                            player.isHost == 1
                        ))
                    );
                }
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return playersList;
        }

        public Player GetPlayer(int idaccount, int idgame) {
            Player player = new Player();
            try {
                using (DostDatabase db = new DostDatabase()) {
                    var playerDb = db.Player.ToList().Find(
                        playerList => playerList.idgame == idgame && playerList.idaccount == idaccount
                    );
                    if (playerDb != null) {
                        player.Id = playerDb.idplayer;
                        player.Account = new Account(
                            playerDb.Account.idaccount, playerDb.Account.username,
                            playerDb.Account.password, playerDb.Account.email,
                            playerDb.Account.coins, playerDb.Account.creationDate,
                            playerDb.Account.isVerified == 1, playerDb.Account.validationCode
                        );
                        player.Score = playerDb.score;
                        player.IsHost = playerDb.isHost == 1;
                        player.Game = null;
                    }
                }
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return player;
        }

        public bool AddPlayer(int idaccount, string guidGame, bool asHost) {
            try {
                var foundGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
                if (foundGame == null) {
                    return false;
                }
                if (foundGame.Players.Count >= MAX_PLAYERS_IN_GAME) {
                    return false;
                }
                using (DostDatabase db = new DostDatabase()) {
                    var newPlayerAccount = db.Account.Find(idaccount);
                    if (newPlayerAccount == null) {
                        return false;
                    }
                    foundGame.Players.Add(new Player {
                        Account = new Account(
                            newPlayerAccount.idaccount, newPlayerAccount.username,
                            newPlayerAccount.password, newPlayerAccount.email,
                            newPlayerAccount.coins, newPlayerAccount.creationDate,
                            newPlayerAccount.isVerified == 1, newPlayerAccount.validationCode
                        ),
                        Game = null,
                        IsHost = asHost,
                        Score = 0,
                        ActivePlayerGuid = Guid.NewGuid().ToString()
                    });
                    return true;
                }
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return false;
        }

        public bool RemovePlayer(string guidPlayer, string guidGame) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            }
            var findPlayer = findGame.Players.Find(player => player.ActivePlayerGuid == guidPlayer);
            if (findPlayer == null) {
                return false;
            }
            if (findPlayer.IsHost && findGame.Players.Count > 1) {
                findGame.Players.Find(player => player.Account.Id != findPlayer.Account.Id).IsHost = true;
            }
            findGame.Players.Remove(findPlayer);
            if (findGame.Players.Count == 0) {
                activeGames.Remove(findGame);
            }
            return true;
        }

        public bool CreateGame(out string guidGame) {
            Game newGame = new Game {
                ActiveGameGuid = Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                Players = new List<Player>(),
                GameCategories = new List<GameCategory>(),
                Round = 0
            };
            Engine.CategoriesList.ForEach((category) => {
                newGame.GameCategories.Add(new GameCategory {
                    Name = category
                });
            });
            activeGames.Add(newGame);
            guidGame = newGame.ActiveGameGuid;
            return true;
        }

        public List<GameCategory> GetCategoriesList(int idgame) {
            List<GameCategory> categoriesList = new List<GameCategory>();
            try {
                using (DostDatabase db = new DostDatabase()) {
                    var categories = (from category in db.GameCategory
                                      where category.idgame == idgame
                                      select category).ToList();
                    categories.ForEach(category => categoriesList.Add(
                        new GameCategory(category.idcategory, null, category.name)
                    ));
                }
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return categoriesList;
        }

        public bool AddCategory(string guidGame, string name) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            }
            var findCategory = findGame.GameCategories.Find(category => category.Name == name);
            if (findCategory != null) {
                return false;
            }
            findGame.GameCategories.Add(new GameCategory {
                Name = name
            });
            return true;
        }

        public bool RemoveCategory(string guidGame, string name) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            }
            var findCategory = findGame.GameCategories.Find(category => category.Name == name);
            if (findCategory == null) {
                return false;
            }
            findGame.GameCategories.Remove(findCategory);
            return true;
        }

        public bool SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            }
            var findPlayer = findGame.Players.Find(player => player.ActivePlayerGuid == guidPlayer);
            if (findPlayer == null) {
                return false;
            }
            findPlayer.IsReady = isPlayerReady;
            return true;
        }

        public bool StartGame(string guidGame) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            } else if (findGame.Round != 0) {
                return false;
            } else if (findGame.Players.Find(player => player.IsReady == false) != null) {
                return false;
            }
            findGame.Round = 1;
            return true;
        }

        public bool SetGameLetter(string guidGame, bool selectRandomLetter, string letter = null) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            } else if (!selectRandomLetter && string.IsNullOrWhiteSpace(letter)) {
                return false;
            }
            findGame.LetterSelected = selectRandomLetter ? Convert.ToChar(new Random().Next(65, 90)).ToString() : letter;
            return true;
        }

        public Game GetActiveGame(string guidGame) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return new Game();
            }
            return findGame;
        }
    }
}
