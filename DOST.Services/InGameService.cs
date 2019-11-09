using DOST.DataAccess;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class InGameService : IInGameService {
        private static readonly Dictionary<string, Dictionary<string, IInGameServiceCallback>> gamesClients =
            new Dictionary<string, Dictionary<string, IInGameServiceCallback>>();
        public static Dictionary<string, Dictionary<string, IInGameServiceCallback>> GamesClients {
            get { return gamesClients; }
        }
        private static Dictionary<string, int> gamesPlayerSelectorIndexHandler = new Dictionary<string, int>();

        public void EnterPlayer(string guidGame, string guidPlayer) {
            if (!gamesClients.ContainsKey(guidGame)) {
                gamesClients.Add(guidGame, new Dictionary<string, IInGameServiceCallback>());
            }
            if (!gamesClients[guidGame].ContainsKey(guidPlayer)) {
                gamesClients[guidGame].Add(guidPlayer, OperationContext.Current.GetCallbackChannel<IInGameServiceCallback>());
            } else {
                gamesClients[guidGame][guidPlayer] = OperationContext.Current.GetCallbackChannel<IInGameServiceCallback>();
            }
        }

        public void LeavePlayer(string guidGame, string guidPlayer) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            if (!gamesClients[guidGame].ContainsKey(guidPlayer)) {
                return;
            }
            gamesClients[guidGame].Remove(guidPlayer);
        }

        public void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            if (!gamesClients[guidGame].ContainsKey(guidPlayer)) {
                return;
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.SetPlayerReady(guidGame, guidPlayer, isPlayerReady);
            }
        }

        public void StartRound(string guidGame, int playerSelectorIndex) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            Game game = GameService.ActiveGames.Find(activeGame => activeGame.ActiveGameGuid == guidGame);
            if (game == null) {
                return;
            }
            if (!gamesPlayerSelectorIndexHandler.ContainsKey(guidGame)) {
                gamesPlayerSelectorIndexHandler.Add(guidGame, 1);
            }
            int playerSelectorIndexHandler = gamesPlayerSelectorIndexHandler[guidGame];
            if (game.Round != 1 || game.Round % game.Players.Count != 0) {
                while (game.Round % (game.Players.Count + playerSelectorIndexHandler) == 0) {
                    playerSelectorIndexHandler += game.Players.Count;
                }
            }
            gamesPlayerSelectorIndexHandler[guidGame] = playerSelectorIndexHandler;
            int nextPlayerIndexLetterSelector = game.Round - playerSelectorIndexHandler;
            foreach (var player in gamesClients[guidGame]) {
                player.Value.StartRound(guidGame, nextPlayerIndexLetterSelector);
            }
        }

        public void StartGame(string guidGame) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.StartGame(guidGame);
            }
        }

        public void EndRound(string guidGame) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.EndRound(guidGame);
            }
        }

        public void PressDost(string guidGame, string guidPlayer) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            if (!gamesClients[guidGame].ContainsKey(guidPlayer)) {
                return;
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.PressDost(guidGame, guidPlayer);
            }
        }

        public void EndGame(string guidGame) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            var game = GameService.ActiveGames.Find(activeGame => activeGame.ActiveGameGuid == guidGame);
            if (game == null) {
                return;
            }
            using (DostDatabase db = new DostDatabase()) {
                var newGame = new DataAccess.Game {
                    date = game.Date,
                    round = game.Round
                };
                db.Game.Add(newGame);
                if (db.SaveChanges() == 0) {
                    return;
                }
                game.Players.ForEach(player => {
                    newGame.Player.Add(new DataAccess.Player {
                        idaccount = player.Account.Id,
                        idgame = newGame.idgame,
                        isHost = player.IsHost ? 1 : 0,
                        score = player.Score
                    });
                });
                if (db.SaveChanges() == 0) {
                    return;
                }
                game.GameCategories.ForEach(category => {
                    var newCategory = new DataAccess.GameCategory {
                        idgame = newGame.idgame,
                        name = category.Name
                    };
                    newGame.GameCategory.Add(newCategory);
                    newGame.Player.ToList().ForEach(player => {
                        var playerCategoryAnswers = category.CategoryPlayerAnswer.Where(categoryAnswer => categoryAnswer.Player.Account.Id == player.idaccount).ToList();
                        playerCategoryAnswers.ForEach(categoryAnswer => {
                            newCategory.CategoryPlayerAnswer.Add(new DataAccess.CategoryPlayerAnswer {
                                idcategory = newCategory.idcategory,
                                idplayer = player.idplayer,
                                answer = categoryAnswer.Answer,
                                round = categoryAnswer.Round
                            });
                        });
                    });
                });
                var playerPlaces = game.Players.ToList();
                playerPlaces.OrderBy(playerPlace => playerPlace.Score);
                for (int index = 0; index < playerPlaces.Count; index++) {
                    db.Account.Find(playerPlaces[index].Account.Id).coins += GameService.MAX_COINS_PER_GAME_WIN / (index + 1);
                }
                if (db.SaveChanges() == 0) {
                    return;
                }
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.EndGame(guidGame);
            }
        }

        public void ReduceTime(string guidGame, string guidPlayer) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            if (!gamesClients[guidGame].ContainsKey(guidPlayer)) {
                return;
            }
            var game = GameService.ActiveGames.Find(playerInGame => playerInGame.ActiveGameGuid == guidGame);
            if (game == null) {
                return;
            }
            var player = game.Players.Find(playerInGame => playerInGame.ActivePlayerGuid == guidPlayer);
            if (player == null) {
                return;
            }
            if (!GameService.GamesTimer.ContainsKey(game.ActiveGameGuid)) {
                return;
            }
            using (DostDatabase db = new DostDatabase()) {
                var account = db.Account.Find(player.Account.Id);
                if (account == null) {
                    return;
                }
                if (account.coins < GameService.ROUND_REDUCE_TIME_COST) {
                    return;
                }
                account.coins -= GameService.ROUND_REDUCE_TIME_COST;
                if (db.SaveChanges() != 1) {
                    return;
                }
            }
            GameService.GamesTimer[game.ActiveGameGuid] = GameService.GamesTimer[game.ActiveGameGuid].AddSeconds(-GameService.ROUND_REDUCE_TIME_SECONDS);
            foreach (var playerInGame in gamesClients[guidGame]) {
                playerInGame.Value.ReduceTime(guidGame);
            }
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class InGameServiceClient : DuplexClientBase<IInGameService>, IInGameService {
        public InGameServiceClient(InstanceContext callbackContext) : base(callbackContext) {
        }

        public void EnterPlayer(string guidGame, string guidPlayer) {
            base.Channel.EnterPlayer(guidGame, guidPlayer);
        }

        public void LeavePlayer(string guidGame, string guidPlayer) {
            base.Channel.LeavePlayer(guidGame, guidPlayer);
        }

        public void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
            base.Channel.SetPlayerReady(guidGame, guidPlayer, isPlayerReady);
        }

        public void StartRound(string guidGame, int playerSelectorIndex) {
            base.Channel.StartRound(guidGame, playerSelectorIndex);
        }

        public void StartGame(string guidGame) {
            base.Channel.StartGame(guidGame);
        }

        public void EndRound(string guidGame) {
            base.Channel.EndRound(guidGame);
        }

        public void PressDost(string guidGame, string guidPlayer) {
            base.Channel.PressDost(guidGame, guidPlayer);
        }

        public void EndGame(string guidGame) {
            base.Channel.EndGame(guidGame);
        }

        public void ReduceTime(string guidGame, string guidPlayer) {
            base.Channel.ReduceTime(guidGame, guidPlayer);
        }
    }
}
