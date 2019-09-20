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

        public void BroadcastMessage(int idpartida, string username, string message) {
            var playersInGame = gamesClients[idpartida];
            foreach (var client in playersInGame) {
                client.Value.BroadcastMessage(idpartida, username, message);
            }
        }

        public void EnterChat(int idpartida, string username) {
            if (!gamesClients.ContainsKey(idpartida)) {
                gamesClients.Add(idpartida, new Dictionary<string, IChatServiceCallback>());
            }
            var playersInGame = gamesClients[idpartida];
            if (!playersInGame.ToList().Exists(client => client.Key == username)) {
                gamesClients[idpartida].Add(username, OperationContext.Current.GetCallbackChannel<IChatServiceCallback>());
            } else {
                gamesClients[idpartida][username] = OperationContext.Current.GetCallbackChannel<IChatServiceCallback>();
            }
        }

        public void LeaveChat(int idpartida, string username) {
            if (!gamesClients.ContainsKey(idpartida)) {
                return;
            } else if (!gamesClients[idpartida].ContainsKey(username)) {
                return;
            }
            gamesClients[idpartida].Remove(username);
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatServiceClient : DuplexClientBase<IChatService>, IChatService {
        public ChatServiceClient(InstanceContext callbackContext) : base(callbackContext) {
        }

        public void BroadcastMessage(int idpartida, string username, string message) {
            base.Channel.BroadcastMessage(idpartida, username, message);
        }

        public void EnterChat(int idpartida, string username) {
            base.Channel.EnterChat(idpartida, username);
        }

        public void LeaveChat(int idpartida, string username) {
            base.Channel.LeaveChat(idpartida, username);
        }
    }
}
