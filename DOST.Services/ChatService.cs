using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChatService {
        private static readonly Dictionary<int, Dictionary<string, IChatServiceCallback>> gamesClients = 
            new Dictionary<int, Dictionary<string, IChatServiceCallback>>();

        public void BroadcastMessage(int idgame, string username, string message) {
            var playersInGame = gamesClients.First(game => game.Key == idgame).Value;
            foreach (var client in playersInGame) {
                client.Value.BroadcastMessage(idgame, username, message);
            }
        }

        public void EnterChat(int idgame, string username) {
            if (!gamesClients.ContainsKey(idgame)) {
                gamesClients.Add(idgame, new Dictionary<string, IChatServiceCallback>());
            }
            var playersInGame = gamesClients[idgame];
            if (!playersInGame.ToList().Exists(client => client.Key == username)) {
                gamesClients[idgame].Add(username, OperationContext.Current.GetCallbackChannel<IChatServiceCallback>());
            } else {
                gamesClients[idgame][username] = OperationContext.Current.GetCallbackChannel<IChatServiceCallback>();
            }
        }

        public void LeaveChat(int idgame, string username) {
            if (!gamesClients.ContainsKey(idgame)) {
                return;
            } else if (!gamesClients[idgame].ContainsKey(username)) {
                return;
            }
            gamesClients[idgame].Remove(username);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatServiceClient : DuplexClientBase<IChatService>, IChatService {
        public ChatServiceClient(InstanceContext callbackContext) : base(callbackContext) {
        }

        public void BroadcastMessage(int idgame, string username, string message) {
            base.Channel.BroadcastMessage(idgame, username, message);
        }

        public void EnterChat(int idgame, string username) {
            base.Channel.EnterChat(idgame, username);
        }

        public void LeaveChat(int idgame, string username) {
            base.Channel.LeaveChat(idgame, username);
        }
    }
}
