using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChatService {
        private static readonly Dictionary<string, Dictionary<string, IChatServiceCallback>> gamesClients = 
            new Dictionary<string, Dictionary<string, IChatServiceCallback>>();

        public void BroadcastMessage(string guidGame, string username, string message) {
            var playersInGame = gamesClients.First(game => game.Key == guidGame).Value;
            foreach (var client in playersInGame) {
                client.Value.BroadcastMessage(guidGame, username, message);
            }
        }

        public void EnterChat(string guidGame, string username) {
            if (!gamesClients.ContainsKey(guidGame)) {
                gamesClients.Add(guidGame, new Dictionary<string, IChatServiceCallback>());
            }
            var playersInGame = gamesClients[guidGame];
            if (!playersInGame.ToList().Exists(client => client.Key == username)) {
                gamesClients[guidGame].Add(username, OperationContext.Current.GetCallbackChannel<IChatServiceCallback>());
            } else {
                gamesClients[guidGame][username] = OperationContext.Current.GetCallbackChannel<IChatServiceCallback>();
            }
        }

        public void LeaveChat(string guidGame, string username) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            } else if (!gamesClients[guidGame].ContainsKey(username)) {
                return;
            }
            gamesClients[guidGame].Remove(username);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatServiceClient : DuplexClientBase<IChatService>, IChatService {
        public ChatServiceClient(InstanceContext callbackContext) : base(callbackContext) {
        }

        public void BroadcastMessage(string guidGame, string username, string message) {
            base.Channel.BroadcastMessage(guidGame, username, message);
        }

        public void EnterChat(string guidGame, string username) {
            base.Channel.EnterChat(guidGame, username);
        }

        public void LeaveChat(string guidGame, string username) {
            base.Channel.LeaveChat(guidGame, username);
        }
    }
}
