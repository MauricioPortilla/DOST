using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameLobbyWindow.xaml
    /// </summary>
    public partial class GameLobbyWindow : Window {
        public bool IsClosed { get; private set; } = false;
        private Game game;
        private Player player;
        private List<TextBlock> lobbyPlayersUsernameTextBlocks;
        private List<TextBlock> lobbyPlayersTypeTextBlocks;
        private List<TextBlock> lobbyPlayersRankTextBlocks;
        private List<TextBlock> lobbyPlayersRankTitleTextBlocks;
        private List<TextBlock> lobbyPlayersReadyStatusTextBlocks;
        private static readonly int MAX_NUMBER_OF_PLAYERS = 4;
        private ChatServiceClient chatService;
        private int actualNumberOfPlayers = 0;

        public GameLobbyWindow(ref Game game) {
            InitializeComponent();
            this.game = game;
            lobbyPlayersUsernameTextBlocks = new List<TextBlock>() {
                playerOneUsernameTextBlock, playerTwoUsernameTextBlock,
                playerThreeUsernameTextBlock, playerFourUsernameTextBlock
            };
            lobbyPlayersTypeTextBlocks = new List<TextBlock>() {
                playerOneTypeTextBlock, playerTwoTypeTextBlock,
                playerThreeTypeTextBlock, playerFourTypeTextBlock
            };
            lobbyPlayersRankTextBlocks = new List<TextBlock>() {
                playerOneRankTextBlock, playerTwoRankTextBlock,
                playerThreeRankTextBlock, playerFourRankTextBlock
            };
            lobbyPlayersRankTitleTextBlocks = new List<TextBlock>() {
                playerOneRankTitleTextBlock, playerTwoRankTitleTextBlock,
                playerThreeRankTitleTextBlock, playerFourRankTitleTextBlock
            };
            lobbyPlayersReadyStatusTextBlocks = new List<TextBlock>() {
                playerOneReadyStatus, playerTwoReadyStatus,
                playerThreeReadyStatus, playerFourReadyStatus
            };
            Thread loadPlayersJoinedDataThread = new Thread(LoadPlayersJoinedData);
            loadPlayersJoinedDataThread.Start();
            InstanceContext chatInstance = new InstanceContext(new ChatCallbackHandler(game, chatListBox));
            chatService = new ChatServiceClient(chatInstance);
            while (true) {
                player = game.Players.Find(playerInGame => playerInGame.Account.Id == Session.Account.Id);
                if (player != null) {
                    chatService.EnterChat(game.ActiveGuidGame, player.Account.Username);
                    break;
                }
            }
        }

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

        public void LoadPlayersJoinedData() {
            while (!IsClosed) {
                this.game = Session.GamesList.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
                if (actualNumberOfPlayers != game.Players.Count) {
                    Application.Current.Dispatcher.Invoke(delegate {
                        PerformUIChanges();
                    });
                }
            }
        }

        private void PerformUIChanges() {
            if (game.Players.Count == 0) {
                return;
            }
            var myPlayer = game.Players.Find(player => player.Account.Id == Session.Account.Id);
            if (myPlayer != null) {
                if (!myPlayer.IsHost) {
                    readyButton.Visibility = Visibility.Visible;
                    startGameButton.Visibility = Visibility.Hidden;
                    configurationButton.Visibility = Visibility.Hidden;
                } else {
                    startGameButton.Visibility = Visibility.Visible;
                    readyButton.Visibility = Visibility.Hidden;
                    configurationButton.Visibility = Visibility.Visible;
                }
            }
            // Can be improved
            for (int index = 0; index < MAX_NUMBER_OF_PLAYERS; index++) {
                lobbyPlayersUsernameTextBlocks[index].Text = "...";
                lobbyPlayersTypeTextBlocks[index].Text = Properties.Resources.WaitingForPlayerText;
                lobbyPlayersRankTextBlocks[index].Text = "#0";
                lobbyPlayersRankTextBlocks[index].Visibility = Visibility.Hidden;
                lobbyPlayersRankTitleTextBlocks[index].Visibility = Visibility.Hidden;
                lobbyPlayersReadyStatusTextBlocks[index].Visibility = Visibility.Hidden;
                lobbyPlayersReadyStatusTextBlocks[index].Text = Properties.Resources.NotReadyText;
            }
            for (int index = 0; index < game.Players.Count; index++) {
                if (lobbyPlayersUsernameTextBlocks[index].Text == game.Players[index].Account.Username) {
                    continue;
                }
                lobbyPlayersUsernameTextBlocks[index].Text = game.Players[index].Account.Username;
                lobbyPlayersTypeTextBlocks[index].Text = game.Players[index].IsHost ?
                    Properties.Resources.HostPlayerText : Properties.Resources.PlayerText;
                lobbyPlayersRankTextBlocks[index].Text = game.Players[index].GetRank();
                lobbyPlayersRankTextBlocks[index].Visibility = Visibility.Visible;
                lobbyPlayersRankTitleTextBlocks[index].Visibility = Visibility.Visible;
                lobbyPlayersReadyStatusTextBlocks[index].Text = game.Players[index].IsReady ? Properties.Resources.ReadyText : Properties.Resources.NotReadyText;
                lobbyPlayersReadyStatusTextBlocks[index].Visibility = Visibility.Visible;
            }
            if (game.Players.Count == MAX_NUMBER_OF_PLAYERS) {
                lobbyStatusTextBlock.Text = "";
            } else {
                lobbyStatusTextBlock.Text = Properties.Resources.WaitingForPlayersText;
            }
            actualNumberOfPlayers = game.Players.Count;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            if (player.LeaveGame(game)) {
                chatService.LeaveChat(game.ActiveGuidGame, player.Account.Username);
                Session.GameLobbyWindow = null;
                Session.MainMenuWindow.Show();
                Close();
            }
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e) {
            if (game.Players.Count < 2) {
                MessageBox.Show(Properties.Resources.MustHaveAtLeastTwoPlayersErrorText);
                return;
            }
            if (!game.Start()) {
                MessageBox.Show(Properties.Resources.StartGameErrorText);
                return;
            }
            Session.GameWindow = new GameWindow(ref game);
            Session.GameWindow.Show();
            Session.GameLobbyWindow = null;
            Close();
        }

        private void ReadyButton_Click(object sender, RoutedEventArgs e) {
            if (player.SetPlayerReady(true)) {
                readyButton.IsEnabled = false;
                PerformUIChanges();
            }
        }

        private void ConfigurationButton_Click(object sender, RoutedEventArgs e) {
            new GameConfigurationWindow(ref game).Show();
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

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
