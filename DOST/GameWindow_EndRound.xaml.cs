using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ServiceModel;

namespace DOST {
    /// <summary>
    /// Represents GameWindow_EndRound.xaml interaction logic.
    /// </summary>
    public partial class GameWindow_EndRound : Window {
        private Game game;
        private Player player;
        private ChatServiceClient chatService;
        private InGameServiceClient inGameService;
        private List<TextBlock> playersStatusTextBlock = new List<TextBlock>();

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        /// <param name="game">Game reference to create lobby</param>
        public GameWindow_EndRound(Game game) {
            InitializeComponent();
            try {
                this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
            } catch (Exception exception) {
                Console.WriteLine("Exception (GameWindow_EndRound) -> " + exception.Message);
                this.game = game;
            }
            try {
                player = game.Players.Find(playerInGame => playerInGame.Account.Id == Session.Account.Id);
                Session.JoinGameChat(game, player, chatListBox, ref chatService);
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(game, this));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameLobbyWindow) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                Close();
                return;
            }
            LoadCategoryPlayerAnswers();
            LoadPlayers();
            endRoundText.Text += " " + game.Round;
            if (player.IsHost) {
                if (game.Round == Session.MAX_ROUNDS_PER_GAME) {
                    showGameResultsButton.Visibility = Visibility.Visible;
                    return;
                }
                startNextRoundButton.Visibility = Visibility.Visible;
            } else {
                readyButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Manages in-game callbacks through network.
        /// </summary>
        public class InGameCallback : InGameCallbackHandler {
            private GameWindow_EndRound window;

            /// <summary>
            /// Creates an instance and initializes it.
            /// </summary>
            /// <param name="game">Game in course</param>
            /// <param name="player">Session player in game</param>
            /// <param name="window">Window where instance will operate</param>
            public InGameCallback(Game game, GameWindow_EndRound window) {
                this.game = game;
                this.window = window;
            }

            /// <summary>
            /// Receives data about a player who changed his ready status and changes it on UI.
            /// </summary>
            /// <param name="guidGame">Game global unique identifier</param>
            /// <param name="guidPlayer">Player global unique identifier</param>
            /// <param name="isPlayerReady">True if player is ready; False if not</param>
            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                if (guidGame == game.ActiveGuidGame) {
                    MessageReceived = true;
                    try {
                        this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
                    } catch (Exception exception) {
                        Console.WriteLine("Exception (GameWindow_EndRound -> SetPlayerReady) -> " + exception.Message);
                    }
                    var playerToInteract = game.Players.Find(playerInGame => playerInGame.ActivePlayerGuid == guidPlayer);
                    if (playerToInteract == null) {
                        return;
                    }
                    int index = game.Players.IndexOf(playerToInteract);
                    if (index < 0) {
                        return;
                    }
                    window.playersStatusTextBlock[index].Foreground = isPlayerReady ? Brushes.LimeGreen : Brushes.White;
                    window.playersStatusTextBlock[index].Opacity = isPlayerReady ? 1 : 0.5;
                }
            }

            public override void StartGame(string guidGame) {
            }

            /// <summary>
            /// Receives data about round that just started.
            /// </summary>
            /// <param name="guidGame">Game global unique identifier</param>
            /// <param name="playerSelectorIndex">Player index selected to select letter</param>
            public override void StartRound(string guidGame, int playerSelectorIndex) {
                if (guidGame == game.ActiveGuidGame) {
                    MessageReceived = true;
                    this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
                    var findHost = game.Players.Find(player => player.IsHost);
                    if (findHost != null) {
                        var findPlayer = game.Players.Find(player => player.Account.Id == Session.Account.Id);
                        new GameWindow_LetterSelection(ref game, ref findPlayer, game.Players[playerSelectorIndex].Account.Id == Session.Account.Id).Show();
                        window.Close();
                    } else {
                        MessageBox.Show(Properties.Resources.NoHostFoundErrorText);
                    }
                }
            }

            public override void EndRound(string guidGame) {
            }

            public override void PressDost(string guidGame, string guidPlayer) {
            }

            /// <summary>
            /// Receives data about game ending.
            /// </summary>
            /// <param name="guidGame">Game global unique identifier</param>
            public override void EndGame(string guidGame) {
                if (guidGame == game.ActiveGuidGame) {
                    MessageReceived = true;
                    window.Close();
                    new GameWindow_EndGame(game).Show();
                }
            }

            public override void ReduceTime(string guidGame) {
            }
        }

        /// <summary>
        /// Loads all category answers for players on UI.
        /// </summary>
        private void LoadCategoryPlayerAnswers() {
            for (int categoryIndex = 0; categoryIndex < game.Categories.Count; categoryIndex++) {
                var categoryStackPanel = new StackPanel {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(15, 11, 15, 5)
                };
                categoryStackPanel.Children.Add(new TextBlock {
                    Text = game.Categories[categoryIndex].Name,
                    FontSize = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                });
                for (int answerIndex = 0; answerIndex < game.Categories[categoryIndex].CategoryPlayerAnswer.Count; answerIndex++) {
                    var playerAnswer = game.Categories[categoryIndex].CategoryPlayerAnswer[answerIndex];
                    if (playerAnswer.Round != game.Round) {
                        continue;
                    }
                    var playerAnswerStackPanel = new StackPanel {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    playerAnswerStackPanel.Children.Add(new TextBlock {
                        Text = playerAnswer.Player.Account.Username + ": ",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 5, 0),
                        Foreground = Brushes.White
                    });
                    playerAnswerStackPanel.Children.Add(new TextBlock {
                        Text = string.IsNullOrWhiteSpace(playerAnswer.Answer) ? "---" : playerAnswer.Answer,
                        Foreground = playerAnswer.HasCorrectAnswer ? Brushes.LightGreen : Brushes.Red
                    });
                    categoryStackPanel.Children.Add(playerAnswerStackPanel);
                }
                playerAnswersResultsStackPanel.Children.Add(categoryStackPanel);
            }
        }

        /// <summary>
        /// Loads players data and their ready status on UI.
        /// </summary>
        private void LoadPlayers() {
            Thickness usernameMargin = new Thickness(20, 10, 0, 0);
            Thickness statusMargin = new Thickness(0, 10, 0, 0);
            for (int index = 0; index < game.Players.Count; index++) {
                var playerStackPanel = new StackPanel {
                    Orientation = Orientation.Horizontal
                };
                playerStackPanel.Children.Add(new TextBlock {
                    Text = game.Players[index].Account.Username,
                    Margin = usernameMargin,
                    Foreground = Brushes.White,
                    Width = 214,
                    Tag = index
                });
                playersStatusTextBlock.Add(new TextBlock {
                    Text = Properties.Resources.ReadyText,
                    Margin = statusMargin,
                    Foreground = Brushes.White,
                    Opacity = 0.5,
                    Tag = index
                });
                playerStackPanel.Children.Add(playersStatusTextBlock.Last());
                playersStackPanel.Children.Add(playerStackPanel);
            }
        }

        /// <summary>
        /// Handles ChatMessageTextBox key enter down. Sends through network a chat message.
        /// </summary>
        /// <param name="sender">TextBox object</param>
        /// <param name="e">TextBox key event</param>
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
        /// Manages window header to enable drag the window.
        /// </summary>
        /// <param name="sender">Window header element</param>
        /// <param name="e">Mouse event handler</param>
        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

        /// <summary>
        /// Handles ReadyButton click event. Sends data to all players to indicate that this player is ready.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void ReadyButton_Click(object sender, RoutedEventArgs e) {
            if (player.IsHost) {
                return;
            }
            EngineNetwork.DoNetworkOperation<CommunicationException>(onExecute: () => {
                if (player.SetPlayerReady(true)) {
                    inGameService.SetPlayerReady(game.ActiveGuidGame, player.ActivePlayerGuid, true);
                    return true;
                }
                return false;
            }, onSuccess: () => {
                Application.Current.Dispatcher.Invoke(delegate {
                    readyButton.IsEnabled = false;
                });
            }, null, true);
        }

        /// <summary>
        /// Handles StartNextRoundButton click event. Sends data to all players to indicate that rounds is about to start.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void StartNextRoundButton_Click(object sender, RoutedEventArgs e) {
            this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
            if (game.Players.Find(playerInGame => playerInGame.IsReady == false && playerInGame.ActivePlayerGuid != player.ActivePlayerGuid) != null) {
                MessageBox.Show(Properties.Resources.PlayersNotReadyErrorText);
                return;
            }
            startNextRoundButton.IsEnabled = false;
            EngineNetwork.DoNetworkOperation(onExecute: () => {
                if (player.SetPlayerReady(true)) {
                    if (!game.Start()) {
                        MessageBox.Show(Properties.Resources.StartGameErrorText);
                        return false;
                    }
                    inGameService.StartRound(game.ActiveGuidGame, 0);
                    return true;
                }
                return false;
            }, null, null, true);
        }

        /// <summary>
        /// Handles ShowGameResultsButton click event. Sends data to all players to indicate that game results is about to show up.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void ShowGameResultsButton_Click(object sender, RoutedEventArgs e) {
            this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
            if (game.Players.Find(playerInGame => playerInGame.IsReady == false && playerInGame.ActivePlayerGuid != player.ActivePlayerGuid) != null) {
                MessageBox.Show(Properties.Resources.PlayersNotReadyErrorText);
                return;
            }
            showGameResultsButton.IsEnabled = false;
            EngineNetwork.DoNetworkOperation(onExecute: () => {
                if (player.SetPlayerReady(true)) {
                    inGameService.EndGame(game.ActiveGuidGame);
                    return true;
                }
                return false;
            }, null, null, true);
        }
    }
}
