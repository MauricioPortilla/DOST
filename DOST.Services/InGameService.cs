﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
            foreach (var player in gamesClients[guidGame]) {
                player.Value.EndGame(guidGame);
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
    }
}
