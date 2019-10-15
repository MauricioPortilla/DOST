﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class InGameService : IInGameService {
        private static readonly Dictionary<string, Dictionary<string, IInGameServiceCallback>> gamesClients =
            new Dictionary<string, Dictionary<string, IInGameServiceCallback>>();

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

        public void StartGame(string guidGame) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            }
            foreach (var player in gamesClients[guidGame]) {
                player.Value.StartGame(guidGame);
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

        public void StartGame(string guidGame) {
            base.Channel.StartGame(guidGame);
        }
    }
}