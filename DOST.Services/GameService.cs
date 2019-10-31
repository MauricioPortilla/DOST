using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using DOST.DataAccess;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GameService : IGameService {
        /// <summary>
        /// Stores active games ID with players lists.
        /// </summary>
        private static readonly List<Game> activeGames = new List<Game>();
        public static List<Game> ActiveGames {
            get { return activeGames; }
        }
        private static readonly int MAX_PLAYERS_IN_GAME = 4;
        private static readonly int ROUND_LETTER_SELECTION_COST = 20;
        private static readonly int ROUND_GET_WORD_COST = 100;

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

        public bool CreateGame(out string guidGame, string language) {
            Game newGame = new Game {
                ActiveGameGuid = Guid.NewGuid().ToString(),
                Date = DateTime.Now,
                Players = new List<Player>(),
                GameCategories = new List<GameCategory>(),
                Round = 0
            };
            Engine.CategoriesList[language].ForEach((category) => {
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

        public bool SetGameLetter(string guidGame, int idaccount, bool selectRandomLetter, string letter = null) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            } else if (!selectRandomLetter && string.IsNullOrWhiteSpace(letter)) {
                return false;
            } else if (!selectRandomLetter) {
                using (DostDatabase db = new DostDatabase()) {
                    var account = db.Account.Find(idaccount);
                    if (account == null) {
                        return false;
                    }
                    account.coins -= ROUND_LETTER_SELECTION_COST;
                    if (db.SaveChanges() != 1) {
                        return false;
                    }
                }
            }
            int asciiLetterA = 65;
            int asciiLetterZ = 90;
            int roundTimeInSeconds = 40 + 3;
            findGame.LetterSelected = selectRandomLetter ? Convert.ToChar(new Random().Next(asciiLetterA, asciiLetterZ)).ToString() : letter;
            findGame.RoundStartingTime = DateTime.Now.Ticks;
            Task.Run(() => {
                var gameTimer = new DateTime(findGame.RoundStartingTime).AddSeconds(roundTimeInSeconds);
                while (gameTimer > DateTime.Now) {
                    continue;
                }
                var gamesClients = InGameService.GamesClients;
                if (!gamesClients.ContainsKey(guidGame)) {
                    return;
                }
                foreach (var gameClient in gamesClients[guidGame]) {
                    gameClient.Value.EndRound(guidGame);
                }
            });
            return true;
        }

        public Game GetActiveGame(string guidGame) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return new Game();
            }
            return findGame;
        }

        public bool SendCategoryAnswers(string guidGame, string guidPlayer, List<CategoryPlayerAnswer> categoryPlayerAnswers) {
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return false;
            }
            var findPlayer = findGame.Players.Find(player => player.ActivePlayerGuid == guidPlayer);
            if (findPlayer == null) {
                return false;
            }
            foreach (var categoryPlayerAnswer in categoryPlayerAnswers) {
                var category = findGame.GameCategories.Find(gameCategory => gameCategory.Name.Equals(categoryPlayerAnswer.GameCategory.Name));
                if (category == null) {
                    continue;
                }
                var playerAnswerToAdd = new CategoryPlayerAnswer {
                    GameCategory = new GameCategory(0, null, category.Name),
                    Answer = categoryPlayerAnswer.Answer,
                    Round = categoryPlayerAnswer.Round,
                    Player = findPlayer
                };
                playerAnswerToAdd.HasCorrectAnswer = EvaluateCategoryPlayerAnswer(playerAnswerToAdd);
                category.CategoryPlayerAnswer.Add(playerAnswerToAdd);
            }
            return true;
        }

        private bool EvaluateCategoryPlayerAnswer(CategoryPlayerAnswer categoryPlayerAnswer) {
            if (string.IsNullOrWhiteSpace(categoryPlayerAnswer.Answer)) {
                return false;
            } else if (!Engine.CategoriesList.Any(list => list.Value.Contains(categoryPlayerAnswer.GameCategory.Name))) {
                return true;
            }
            try {
                var categoryFileWords = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\Categories\\" + categoryPlayerAnswer.GameCategory.Name + ".txt").ToList();
                if (!categoryFileWords.Exists(word => word.ToLower() == categoryPlayerAnswer.Answer.ToLower())) {
                    return false;
                }
            } catch (FileNotFoundException fileNotFoundException) {
                Console.WriteLine("FileNotFoundException (EvaluateCategoryPlayerAnswer) -> " + fileNotFoundException.Message);
            } catch (IOException ioException) {
                Console.WriteLine("IOException (EvaluateCategoryPlayerAnswer) -> " + ioException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception (EvaluateCategoryPlayerAnswer) -> " + exception.Message);
            }
            return true;
        }

        public string GetCategoryWord(string guidGame, string guidPlayer, string categoryName) {
            if (!Engine.CategoriesList.Any(list => list.Value.Contains(categoryName))) {
                return string.Empty;
            }
            var findGame = activeGames.Find(game => game.ActiveGameGuid == guidGame);
            if (findGame == null) {
                return string.Empty;
            }
            var findPlayer = findGame.Players.Find(player => player.ActivePlayerGuid == guidPlayer);
            if (findPlayer == null) {
                return string.Empty;
            }
            try {
                var categoryFileWords = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\Categories\\" + categoryName + ".txt").ToList();
                var matchedWords = categoryFileWords.Where(word => word[0] == Convert.ToChar(findGame.LetterSelected))
                    .Union(categoryFileWords.Where(lowerWord => lowerWord[0] == Convert.ToChar(findGame.LetterSelected.ToLower()))).ToList();
                var wordFound = matchedWords[new Random().Next(0, matchedWords.Count() - 1)];
                findPlayer.Score -= ROUND_GET_WORD_COST;
                return wordFound.ToUpperInvariant();
            } catch (FileNotFoundException fileNotFoundException) {
                Console.WriteLine("FileNotFoundException (EvaluateCategoryPlayerAnswer) -> " + fileNotFoundException.Message);
            } catch (IOException ioException) {
                Console.WriteLine("IOException (EvaluateCategoryPlayerAnswer) -> " + ioException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception (EvaluateCategoryPlayerAnswer) -> " + exception.Message);
            }
            return string.Empty;
        }
    }
}
