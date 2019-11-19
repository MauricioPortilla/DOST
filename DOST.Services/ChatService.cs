using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace DOST.Services {
    /// <summary>
    /// Manages chat service operations through network.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ChatService : IChatService {
        /// <summary>
        /// Represents user connections in every game.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, IChatServiceCallback>> gamesClients = 
            new Dictionary<string, Dictionary<string, IChatServiceCallback>>();
        public static Dictionary<string, Dictionary<string, IChatServiceCallback>> GamesClients {
            get { return gamesClients; }
        }

        /// <summary>
        /// Broadcasts to all clients connected to the chat in a specific game a new message.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="username">Username who sends the message</param>
        /// <param name="message">Message to be send</param>
        public void BroadcastMessage(string guidGame, string username, string message) {
            var playersInGame = gamesClients.First(game => game.Key == guidGame).Value;
            foreach (var client in playersInGame) {
                client.Value.BroadcastMessage(guidGame, username, message);
            }
        }

        /// <summary>
        /// Stores a new connection for a game chat.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="username">Username who joins to the chat</param>
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

        /// <summary>
        /// Removes a connection for a game chat.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="username">Username who leaves the chat</param>
        public void LeaveChat(string guidGame, string username) {
            if (!gamesClients.ContainsKey(guidGame)) {
                return;
            } else if (!gamesClients[guidGame].ContainsKey(username)) {
                return;
            }
            gamesClients[guidGame].Remove(username);
        }
    }

    /// <summary>
    /// Manages chat service callbacks operations through network.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ChatServiceClient : DuplexClientBase<IChatService>, IChatService {
        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        /// <param name="callbackContext"></param>
        public ChatServiceClient(InstanceContext callbackContext) : base(callbackContext) {
        }

        /// <summary>
        /// Sends to chat service information about the message to be broadcasted.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="username">Username who sends the message</param>
        /// <param name="message">Message to be send</param>
        public void BroadcastMessage(string guidGame, string username, string message) {
            base.Channel.BroadcastMessage(guidGame, username, message);
        }

        /// <summary>
        /// Sends to chat service information about a new joining.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="username">Username who joins to the chat</param>
        public void EnterChat(string guidGame, string username) {
            base.Channel.EnterChat(guidGame, username);
        }

        /// <summary>
        /// Sends to chat service information about a new leaving.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="username">Username who leaves the chat</param>
        public void LeaveChat(string guidGame, string username) {
            base.Channel.LeaveChat(guidGame, username);
        }
    }
}
