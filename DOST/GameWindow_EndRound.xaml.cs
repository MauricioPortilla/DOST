using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ServiceModel;
using static DOST.GameLobbyWindow;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameWindow_EndRound.xaml
    /// </summary>
    public partial class GameWindow_EndRound : Window {
        private Game game;
        private Player player;
        private ChatServiceClient chatService;
        private InGameServiceClient inGameService;

        public GameWindow_EndRound(Game game) {
            InitializeComponent();
            this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
            try {
                InstanceContext chatInstance = new InstanceContext(new ChatCallbackHandler(game, chatListBox));
                chatService = new ChatServiceClient(chatInstance);
                while (true) {
                    player = game.Players.Find(playerInGame => playerInGame.Account.Id == Session.Account.Id);
                    if (player != null) {
                        chatService.EnterChat(game.ActiveGuidGame, player.Account.Username);
                        break;
                    }
                }
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(game, player));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameLobbyWindow) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                Close();
            }
        }

        public class InGameCallback : InGameCallbackHandler {
            private Player player;

            public InGameCallback(Game game, Player player) {
                this.game = game;
                this.player = player;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                throw new NotImplementedException();
            }

            public override void StartGame(string guidGame) {
                throw new NotImplementedException();
            }

            public override void StartRound(string guidGame) {
                throw new NotImplementedException();
            }

            public override void EndRound(string guidGame) {
                throw new NotImplementedException();
            }
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (string.IsNullOrWhiteSpace(chatMessageTextBox.Text)) {
                    return;
                }
                chatService.BroadcastMessage(game.ActiveGuidGame, player.Account.Username, chatMessageTextBox.Text);
                chatMessageTextBox.Clear();
            }
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
