using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatService : IChatService {
        private static readonly Dictionary<string, IChatServiceCallback> clients = new Dictionary<string, IChatServiceCallback>();
        public void BroadcastMessage(string username, string message) {
            foreach (var client in clients) {
                client.Value.BroadcastMessage(username, message);
            }
        }

        public void EnterChat(string username) {
            if (!clients.ToList().Exists(client => client.Key == username)) {
                clients.Add(username, OperationContext.Current.GetCallbackChannel<IChatServiceCallback>());
            }
        }
    }

    public class ChatServiceClient : DuplexClientBase<IChatService>, IChatService {
        public ChatServiceClient(InstanceContext callbackContext) : base(callbackContext) {
        }

        public void BroadcastMessage(string username, string message) {
            base.Channel.BroadcastMessage(username, message);
        }

        public void EnterChat(string username) {
            base.Channel.EnterChat(username);
        }
    }
}
