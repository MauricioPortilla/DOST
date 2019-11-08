using DOST.Services;
using System.Windows.Controls;

namespace DOST {

    /// <summary>
    /// Handles client-side callback operations for the chat service.
    /// </summary>
    public class ChatCallbackHandler : IChatServiceCallback {
        private Game game;
        private ListBox chatListBox;
        public string LastMessageReceived;

        /// <summary>
        /// Creates a ChatCallbackHandler instance given a game and a chat list box.
        /// </summary>
        /// <param name="game">Game where the chat belongs to</param>
        /// <param name="chatListBox">ListBox where the messages will be placed</param>
        public ChatCallbackHandler(Game game, ListBox chatListBox) {
            this.game = game;
            this.chatListBox = chatListBox;
        }

        /// <summary>
        /// Receives a chat message and places it into the chat box.
        /// </summary>
        /// <param name="guidGame">Game global unique identifier</param>
        /// <param name="username">Player username who sent the message</param>
        /// <param name="message">Message content</param>
        public void BroadcastMessage(string guidGame, string username, string message) {
            if (guidGame == game.ActiveGuidGame) {
                LastMessageReceived = message;
                chatListBox.Items.Add(new TextBlock() {
                    Text = username + ": " + message
                });
                chatListBox.ScrollIntoView(chatListBox.Items[chatListBox.Items.Count - 1]);
            }
        }
    }
}
