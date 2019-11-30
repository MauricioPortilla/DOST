using DOST.Services;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DOST {

    /// <summary>
    /// Represents GameLobbyWindow.xaml interaction logic.
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

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        /// <param name="game">Game reference to create lobby</param>
        public GameLobbyWindow(ref Game game) {
            Session.IsPlayerInGame = true;
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
                player = game.Players.Find(playerInGame => playerInGame.Account.Id == Session.Account.Id);
                Session.JoinGameChat(game, player, chatListBox, ref chatService);
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(game, ref lobbyPlayersReadyStatusTextBlocks));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameLobbyWindow) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                Close();
            }
        }

        /// <summary>
        /// Manages client-side in-game callback.
        /// </summary>
        public class InGameCallback : InGameCallbackHandler {
            private readonly List<TextBlock> lobbyPlayersReadyStatusTextBlocks;

            /// <summary>
            /// Creates an instance for a given game.
            /// </summary>
            /// <param name="game">Game where this callback belongs to</param>
            /// <param name="lobbyPlayersReadyStatusTextBlocks">List of all players' ready status</param>
            public InGameCallback(Game game, ref List<TextBlock> lobbyPlayersReadyStatusTextBlocks) {
                this.game = game;
                this.lobbyPlayersReadyStatusTextBlocks = lobbyPlayersReadyStatusTextBlocks;
            }

            /// <summary>
            /// Receives data from a player who just went ready and changes his ready status.
            /// </summary>
            /// <param name="guidGame">Game global unique identifier</param>
            /// <param name="guidPlayer">Player global unique identifier</param>
            /// <param name="isPlayerReady">Player ready status</param>
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

            /// <summary>
            /// Receives data from in-game service to indicate the round starting.
            /// </summary>
            /// <param name="guidGame">Game global unique identifier</param>
            /// <param name="playerSelectorIndex">Player index selected to select letter</param>
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

            public override void ReduceTime(string guidGame) {
            }
        }

        /// <summary>
        /// Reloads players data in UI if the number of players in game changed.
        /// </summary>
        private void LoadPlayersJoinedData() {
            var activeGuidGame = game.ActiveGuidGame;
            while (!IsClosed) {
                try {
                    List<Game> games = Session.AllGamesAvailable;
                    this.game = games.Find(gameInList => gameInList.ActiveGuidGame == activeGuidGame);
                    if (game == null) {
                        continue;
                    }
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

        /// <summary>
        /// Reloads all the UI data.
        /// </summary>
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
            ResetLobbyUI();
            for (int index = 0; index < game.Players.Count; index++) {
                if (lobbyPlayersUsernameTextBlocks[index].Text == game.Players[index].Account.Username) {
                    continue;
                }
                lobbyPlayersUsernameTextBlocks[index].Text = game.Players[index].Account.Username;
                lobbyPlayersTypeTextBlocks[index].Text = game.Players[index].IsHost ? Properties.Resources.HostPlayerText : Properties.Resources.PlayerText;
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

        /// <summary>
        /// Resets UI data to default values.
        /// </summary>
        private void ResetLobbyUI() {
            for (int index = 0; index < Session.MAX_PLAYERS_IN_GAME; index++) {
                lobbyPlayersUsernameTextBlocks[index].Text = "...";
                lobbyPlayersTypeTextBlocks[index].Text = Properties.Resources.WaitingForPlayerText;
                lobbyPlayersRankTextBlocks[index].Text = "#0";
                lobbyPlayersRankTextBlocks[index].Visibility = Visibility.Hidden;
                lobbyPlayersRankTitleTextBlocks[index].Visibility = Visibility.Hidden;
                lobbyPlayersReadyStatusTextBlocks[index].Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Handles ExitButton click event.
        /// </summary>
        /// <param name="sender">ExitButton object</param>
        /// <param name="e">Button click event</param>
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

        /// <summary>
        /// Handles StartGameButton click event.
        /// </summary>
        /// <param name="sender">StartGameButton object</param>
        /// <param name="e">Button click event</param>
        private void StartGameButton_Click(object sender, RoutedEventArgs e) {
            if (game.Players.Count < 2) {
                MessageBox.Show(Properties.Resources.MustHaveAtLeastTwoPlayersErrorText);
                return;
            }
            if (game.Players.Find(playerInGame => !playerInGame.IsReady && playerInGame.ActivePlayerGuid != player.ActivePlayerGuid) != null) {
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

        /// <summary>
        /// Handles ReadyButton click event.
        /// </summary>
        /// <param name="sender">ReadyButton object</param>
        /// <param name="e">Button click event</param>
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

        /// <summary>
        /// Handles ConfigurationButton click event.
        /// </summary>
        /// <param name="sender">ConfigurationButton object</param>
        /// <param name="e">Button click event</param>
        private void ConfigurationButton_Click(object sender, RoutedEventArgs e) {
            new GameConfigurationWindow(ref game).Show();
        }

        /// <summary>
        /// Sends a chat message on enter key down.
        /// </summary>
        /// <param name="sender">ChatMessageTextBox object</param>
        /// <param name="e">Key event</param>
        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (string.IsNullOrWhiteSpace(chatMessageTextBox.Text)) {
                    return;
                }
                EngineNetwork.DoNetworkOperation<CommunicationException>(onExecute: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        chatService.BroadcastMessage(game.ActiveGuidGame, player.Account.Username, chatMessageTextBox.Text);
                        chatMessageTextBox.Clear();
                    });
                    return true;
                });
            }
        }

        /// <summary>
        /// Executed when window was closed.
        /// </summary>
        /// <param name="e">Window event</param>
        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
        }

        /// <summary>
        /// Manages window header to enable drag the window.
        /// </summary>
        /// <param name="sender">Window header element</param>
        /// <param name="e">Mouse event handler</param>
        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
