using DOST.Services;
using System.Windows.Controls;

namespace DOST {
    public class ChatCallbackHandler : IChatServiceCallback {
        private Game game;
        private ListBox chatListBox;
        public string LastMessageReceived;

        public ChatCallbackHandler(Game game, ListBox chatListBox) {
            this.game = game;
            this.chatListBox = chatListBox;
        }

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
