using DOST.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.ServiceModel;
using System.Threading;
using System.Windows;

namespace DOST {
    /// <summary>
    /// Represents GameWindow_LetterSelection.xaml interaction logic.
    /// </summary>
    public partial class GameWindow_LetterSelection : Window {
        public bool IsClosed { get; private set; } = false;
        private Game game;
        private Player player;
        private bool showLetterSelectionOptions = false;
        private InGameServiceClient inGameService;

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        /// <param name="game">Game to be used</param>
        /// <param name="player">Player ingame</param>
        /// <param name="showLetterSelectionOptions">True if should show letter selection options; False if not</param>
        public GameWindow_LetterSelection(ref Game game, ref Player player, bool showLetterSelectionOptions) {
            InitializeComponent();
            this.game = game;
            this.player = player;
            this.showLetterSelectionOptions = showLetterSelectionOptions;
            try {
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(game, this));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameWindow_LetterSelection) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                return;
            }
            ShowLetterSelectionOptions(showLetterSelectionOptions);
        }

        /// <summary>
        /// Manages in-game callbacks through network.
        /// </summary>
        public class InGameCallback : InGameCallbackHandler {
            private GameWindow_LetterSelection window;

            /// <summary>
            /// Creates an instance and initializes it.
            /// </summary>
            /// <param name="game">Game in course</param>
            /// <param name="window">Window where instance will operate</param>
            public InGameCallback(Game game, GameWindow_LetterSelection window) {
                this.game = game;
                this.window = window;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
            }

            public override void StartRound(string guidGame, int playerSelectorIndex) {
            }

            /// <summary>
            /// Receives data about a game that just started.
            /// </summary>
            /// <param name="guidGame">Game global unique identifier</param>
            public override void StartGame(string guidGame) {
                if (game.ActiveGuidGame == guidGame) {
                    MessageReceived = true;
                    window.StartGame();
                }
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
        /// Opens a new GameWindow.
        /// </summary>
        private void StartGame() {
            DialogHost.Show(loadingStackPanel, "GameWindow_LetterSelection_WindowDialogHost", (openSender, openEventArgs) => {
                EngineNetwork.DoNetworkOperation(onExecute: () => {
                    Thread.Sleep(2000); // allows session thread to load latest game data
                    return true;
                }, onSuccess: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        openEventArgs.Session.Close(true);
                        Session.GameWindow = new GameWindow(game);
                        Session.GameWindow.Show();
                        Close();
                    });
                }, onFinish: null, false);
            }, null);
        }

        /// <summary>
        /// Manages letter selection options on UI.
        /// </summary>
        /// <param name="show">True if should show options; False if not</param>
        public void ShowLetterSelectionOptions(bool show) {
            if (show) {
                letterSelectionOptionsGrid.Visibility = Visibility.Visible;
                waitingForLetterSelectionGrid.Visibility = Visibility.Hidden;
            } else {
                letterSelectionOptionsGrid.Visibility = Visibility.Hidden;
                waitingForLetterSelectionGrid.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles SelectRandomLetterButton click event. Sends through network data to indicate that a random letter
        /// was set and game should start.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void SelectRandomLetterButton_Click(object sender, RoutedEventArgs e) {
            IsEnabled = false;
            EngineNetwork.DoNetworkOperation<CommunicationException>(onExecute: () => {
                if (game.SetLetter(true, Session.Account.Id)) {
                    inGameService.StartGame(game.ActiveGuidGame);
                    return true;
                } else {
                    Application.Current.Dispatcher.Invoke(delegate {
                        MessageBox.Show(Properties.Resources.CouldntSelectLetterErrorText);
                        IsEnabled = true;
                    });
                    return false;
                }
            }, null, null, true);
        }

        /// <summary>
        /// Handles SelectSpecificLetterButton click event. Loads all the letters that could be chosen.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void SelectSpecificLetterButton_Click(object sender, RoutedEventArgs e) {
            if (Session.Account.Coins < Session.ROUND_LETTER_SELECTION_COST) {
                MessageBox.Show(Properties.Resources.YouDontHaveEnoughCoinsErrorText);
                return;
            }
            letterComboBox.Items.Clear();
            int asciiLetterA = 65;
            int asciiLetterZ = 90;
            for (int letterASCII = asciiLetterA; letterASCII <= asciiLetterZ; letterASCII++) {
                letterComboBox.Items.Add(Convert.ToChar(letterASCII).ToString());
            }
            letterSelectionOptionsGrid.Visibility = Visibility.Hidden;
            letterSelectionSelectorGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles SelectLetterButton click event. Sends through network data to indicate that a specific letter
        /// was set and game should start.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void SelectLetterButton_Click(object sender, RoutedEventArgs e) {
            if (letterComboBox.SelectedItem == null) {
                MessageBox.Show(Properties.Resources.MustSelectALetterErrorText);
                return;
            }
            IsEnabled = false;
            if (game.SetLetter(false, Session.Account.Id, letterComboBox.SelectedItem.ToString())) {
                Session.Account.Coins -= Session.ROUND_LETTER_SELECTION_COST;
                inGameService.StartGame(game.ActiveGuidGame);
                return;
            }
            MessageBox.Show(Properties.Resources.CouldntSelectLetterErrorText);
            IsEnabled = true;
        }

        /// <summary>
        /// Closes actual window.
        /// </summary>
        /// <param name="e">Window event</param>
        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
