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
        private ChatServiceClient chatService;
        private InGameServiceClient inGameService;
        private int actualNumberOfPlayers = 0;

        public GameLobbyWindow(ref Game game) {
            Session.IsPlayerInGame = true;
            Session.Game_ForPlayerIndex = 1;
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
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(game, ref lobbyPlayersReadyStatusTextBlocks));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameLobbyWindow) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                Close();
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

        public class InGameCallback : InGameCallbackHandler {
            private readonly List<TextBlock> lobbyPlayersReadyStatusTextBlocks;

            public InGameCallback(Game game, ref List<TextBlock> lobbyPlayersReadyStatusTextBlocks) {
                this.game = game;
                this.lobbyPlayersReadyStatusTextBlocks = lobbyPlayersReadyStatusTextBlocks;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                if (guidGame == game.ActiveGuidGame) {
                    var playerToInteract = game.Players.Find(playerInGame => playerInGame.ActivePlayerGuid == guidPlayer);
                    if (playerToInteract == null) {
                        return;
                    }
                    int index = game.Players.IndexOf(playerToInteract);
                    if (index < 0) {
                        return;
                    }
                    lobbyPlayersReadyStatusTextBlocks[index].Text = isPlayerReady ? Properties.Resources.ReadyText : Properties.Resources.NotReadyText;
                }
            }

            public override void StartRound(string guidGame, int playerSelectorIndex) {
                if (guidGame == game.ActiveGuidGame) {
                    var findHost = game.Players.Find(player => player.IsHost);
                    if (findHost != null) {
                        var findPlayer = game.Players.Find(player => player.Account.Id == Session.Account.Id);
                        new GameWindow_LetterSelection(ref game, ref findPlayer, findHost.Account.Id == Session.Account.Id).Show();
                    } else {
                        MessageBox.Show(Properties.Resources.NoHostFoundErrorText);
                    }
                    Session.GameLobbyWindow.Close();
                    Session.GameLobbyWindow = null;
                }
            }

            public override void StartGame(string guidGame) {
            }

            public override void EndRound(string guidGame) {
            }

            public override void PressDost(string guidGame, string guidPlayer) {
            }

            public override void EndGame(string guidGame) {
            }
        }

        private void LoadPlayersJoinedData() {
            while (!IsClosed) {
                try {
                    List<Game> games = Session.AllGamesAvailable;
                    this.game = games.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
                    if (actualNumberOfPlayers != game.Players.Count) {
                        Application.Current.Dispatcher.Invoke(delegate {
                            PerformLobbyUIChanges();
                        });
                    }
                } catch (InvalidOperationException invalidOperationException) {
                    Console.WriteLine("Invalid operation exception (LoadPlayersJoinedData) -> " + invalidOperationException.Message);
                }
            }
        }

        private void PerformLobbyUIChanges() {
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
            for (int index = 0; index < Session.MAX_PLAYERS_IN_GAME; index++) {
                lobbyPlayersUsernameTextBlocks[index].Text = "...";
                lobbyPlayersTypeTextBlocks[index].Text = Properties.Resources.WaitingForPlayerText;
                lobbyPlayersRankTextBlocks[index].Text = "#0";
                lobbyPlayersRankTextBlocks[index].Visibility = Visibility.Hidden;
                lobbyPlayersRankTitleTextBlocks[index].Visibility = Visibility.Hidden;
                lobbyPlayersReadyStatusTextBlocks[index].Visibility = Visibility.Hidden;
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
            if (game.Players.Count == Session.MAX_PLAYERS_IN_GAME) {
                lobbyStatusTextBlock.Text = "";
            } else {
                lobbyStatusTextBlock.Text = Properties.Resources.WaitingForPlayersText;
            }
            actualNumberOfPlayers = game.Players.Count;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            if (player.LeaveGame(game)) {
                try {
                    chatService.LeaveChat(game.ActiveGuidGame, player.Account.Username);
                    inGameService.LeavePlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
                    Session.IsPlayerInGame = false;
                    Session.GameLobbyWindow = null;
                    Session.MainMenuWindow.Show();
                    Close();
                } catch (CommunicationException communicationException) {
                    Console.WriteLine("CommunicationException (ExitButton_Click) -> " + communicationException.Message);
                    MessageBox.Show(Properties.Resources.AnErrorHasOcurredErrorText);
                }
            }
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e) {
            if (game.Players.Count < 2) {
                MessageBox.Show(Properties.Resources.MustHaveAtLeastTwoPlayersErrorText);
                return;
            }
            if (game.Players.Find(playerInGame => playerInGame.IsReady == false && playerInGame.ActivePlayerGuid != player.ActivePlayerGuid) != null) {
                MessageBox.Show(Properties.Resources.PlayersNotReadyErrorText);
                return;
            } else if (player.SetPlayerReady(true)) {
                if (!game.Start()) {
                    MessageBox.Show(Properties.Resources.StartGameErrorText);
                    return;
                }
            }
            inGameService.StartRound(game.ActiveGuidGame, 0);
        }

        private void ReadyButton_Click(object sender, RoutedEventArgs e) {
            if (player.IsHost) {
                return;
            } else if (player.SetPlayerReady(true)) {
                try {
                    readyButton.IsEnabled = false;
                    inGameService.SetPlayerReady(game.ActiveGuidGame, player.ActivePlayerGuid, true);
                } catch (CommunicationException communicationException) {
                    Console.WriteLine("CommunicationException (ReadyButton_Click) -> " + communicationException.Message);
                    MessageBox.Show(Properties.Resources.AnErrorHasOcurredErrorText);
                    readyButton.IsEnabled = true;
                }
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
