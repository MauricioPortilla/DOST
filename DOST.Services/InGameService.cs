using DOST.DataAccess;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace DOST.Services {
    /// <summary>
    /// Manages ingame operations through network.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class InGameService : IInGameService {
        /// <summary>
        ///     Stores active players in games. Stores guidGame as key and a value as a dictionary
        ///     where key is guidPlayer and value is player connection.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, IInGameServiceCallback>> gamesClients =
            new Dictionary<string, Dictionary<string, IInGameServiceCallback>>();
        public static Dictionary<string, Dictionary<string, IInGameServiceCallback>> GamesClients {
            get { return gamesClients; }
        }
        /// <summary>
        /// Manages player letter selectors for each active game. Key is the guidGame and value is the index.
        /// </summary>
        private static Dictionary<string, int> gamesPlayerSelectorIndexHandler = new Dictionary<string, int>();
        private static Task checkClientsConnectionStatus;

        /// <summary>
        /// Creates an instance and initializes it. Loads check clients connection status method.
        /// </summary>
        public InGameService() {
            if (checkClientsConnectionStatus == null || checkClientsConnectionStatus.Status == TaskStatus.Faulted) {
                checkClientsConnectionStatus = Task.Run(() => {
                    CheckClientsConnectionStatus();
                });
            }
        }

        /// <summary>
        /// Verifies connection status of every client connection of every active game. If any connection is closed, it will be removed
        /// from respective active game and from connections list.
        /// </summary>
        private void CheckClientsConnectionStatus() {
            var connectionsToRemove = new List<string>();
            while (true) {
                try {
                    foreach (var gameClient in gamesClients) {
                        connectionsToRemove.Clear();
                        foreach (var activeGameClient in gameClient.Value) {
                            if (((ICommunicationObject) activeGameClient.Value).State == CommunicationState.Closed) {
                                connectionsToRemove.Add(activeGameClient.Key);
                            }
                        }
                        foreach (var connectionToRemove in connectionsToRemove) {
                            var gameOwner = GameService.ActiveGames.Find(game => game.Players.Find(player => player.ActivePlayerGuid == connectionToRemove) != null);
                            if (gameOwner == null) {
                                continue;
                            }
                            var playerOwner = gameOwner.Players.Find(player => player.ActivePlayerGuid == connectionToRemove);
                            if (playerOwner == null) {
                                continue;
                            }
                            var isPlayerHost = playerOwner.IsHost;
                            gameOwner.Players.Remove(playerOwner);
                            if (gameOwner.Players.Count > 0) {
                                gameOwner.Players.First().IsHost = isPlayerHost;
                            }
                            if (ChatService.GamesClients[gameOwner.ActiveGameGuid].ContainsKey(playerOwner.Account.Username)) {
                                ChatService.GamesClients[gameOwner.ActiveGameGuid].Remove(playerOwner.Account.Username);
                            }
                            gameClient.Value.Remove(connectionToRemove);
                        }
                    }
                } catch (System.Exception e) {
                    System.Console.WriteLine("Exception (InGameService) -> " + e.Message);
                }
                System.Threading.Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Joins a new player to an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
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

        /// <summary>
        /// Removes a player from an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
        public void LeavePlayer(string guidGame, string guidPlayer) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            if (!gamesClients[guidGame].ContainsKey(guidPlayer)) {
                return;
            }
            gamesClients[guidGame].Remove(guidPlayer);
        }

        /// <summary>
        /// Sets a player ready status in an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
        /// <param name="isPlayerReady">True if player is ready; False if not</param>
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

        /// <summary>
        /// Starts a new round in an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="playerSelectorIndex">Player letter selector index</param>
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
            if (game.Players.Count > 1) {
                if (game.Round != 1 || game.Round % game.Players.Count != 0) {
                    while (game.Round % (game.Players.Count + playerSelectorIndexHandler) == 0) {
                        playerSelectorIndexHandler += game.Players.Count;
                    }
                }
            } else {
                playerSelectorIndexHandler = game.Round;
            }
            gamesPlayerSelectorIndexHandler[guidGame] = playerSelectorIndexHandler;
            int nextPlayerIndexLetterSelector = game.Round - playerSelectorIndexHandler;
            foreach (var player in gamesClients[guidGame]) {
                player.Value.StartRound(guidGame, nextPlayerIndexLetterSelector);
            }
        }

        /// <summary>
        /// Starts a new game in an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        public void StartGame(string guidGame) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.StartGame(guidGame);
            }
        }

        /// <summary>
        /// Ends a round in an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        public void EndRound(string guidGame) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.EndRound(guidGame);
            }
        }

        /// <summary>
        /// Indicates that a player pressed dost button in an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
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

        /// <summary>
        /// Ends the game in an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
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
                playerPlaces = playerPlaces.OrderBy(playerPlace => playerPlace.Score).ToList();
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
            System.Threading.Thread.Sleep(2000);
            gamesClients.Remove(guidGame);
            GameService.ActiveGames.Remove(game);
        }

        /// <summary>
        /// Reduces the timer time of an ingame environment.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
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

    /// <summary>
    /// Manages ingame service callbacks operations through network.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class InGameServiceClient : DuplexClientBase<IInGameService>, IInGameService {
        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        /// <param name="callbackContext"></param>
        public InGameServiceClient(InstanceContext callbackContext) : base(callbackContext) {
        }

        /// <summary>
        /// Sends to ingame service information about the player that is about to join to the game.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
        public void EnterPlayer(string guidGame, string guidPlayer) {
            base.Channel.EnterPlayer(guidGame, guidPlayer);
        }

        /// <summary>
        /// Sends to ingame service information about the player that is about to leave the game.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
        public void LeavePlayer(string guidGame, string guidPlayer) {
            base.Channel.LeavePlayer(guidGame, guidPlayer);
        }

        /// <summary>
        /// Sends to ingame service information about the player that is about to change his ready status.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
        /// <param name="isPlayerReady">True if player is ready; False if not</param>
        public void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
            base.Channel.SetPlayerReady(guidGame, guidPlayer, isPlayerReady);
        }

        /// <summary>
        /// Sends to ingame service information about the round that is about to start.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="playerSelectorIndex">Player letter selector index</param>
        public void StartRound(string guidGame, int playerSelectorIndex) {
            base.Channel.StartRound(guidGame, playerSelectorIndex);
        }

        /// <summary>
        /// Sends to ingame service information about the game that is about to start.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        public void StartGame(string guidGame) {
            base.Channel.StartGame(guidGame);
        }

        /// <summary>
        /// Sends to ingame service information about the round that is about to end.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        public void EndRound(string guidGame) {
            base.Channel.EndRound(guidGame);
        }

        /// <summary>
        /// Sends to ingame service information about the player that pressed the dost button.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
        public void PressDost(string guidGame, string guidPlayer) {
            base.Channel.PressDost(guidGame, guidPlayer);
        }

        /// <summary>
        /// Sends to ingame service information about the game that is about to end.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        public void EndGame(string guidGame) {
            base.Channel.EndGame(guidGame);
        }

        /// <summary>
        /// Sends to ingame service information about the time reducing in a game.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="guidPlayer">Player global unique identifier</param>
        public void ReduceTime(string guidGame, string guidPlayer) {
            base.Channel.ReduceTime(guidGame, guidPlayer);
        }
    }
}
