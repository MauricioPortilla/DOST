using DOST.Services;
using MaterialDesignThemes.Wpf;
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
    /// Lógica de interacción para GameWindow_LetterSelection.xaml
    /// </summary>
    public partial class GameWindow_LetterSelection : Window {
        public bool IsClosed { get; private set; } = false;
        private Game game;
        private Player player;
        private bool showLetterSelectionOptions = false;
        private InGameServiceClient inGameService;

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

        public class InGameCallback : InGameCallbackHandler {
            private GameWindow_LetterSelection window;

            public InGameCallback(Game game, GameWindow_LetterSelection window) {
                this.game = game;
                this.window = window;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                throw new NotImplementedException();
            }

            public override void StartRound(string guidGame) {
                throw new NotImplementedException();
            }

            public override void StartGame(string guidGame) {
                if (game.ActiveGuidGame == guidGame) {
                    window.StartGame();
                }
            }

            public override void EndRound(string guidGame) {
                throw new NotImplementedException();
            }
        }

        private void StartGame() {
            DialogHost.Show(loadingStackPanel, "GameWindow_LetterSelection_WindowDialogHost", (openSender, openEventArgs) => {
                EngineNetwork.DoNetworkAction(onExecute: () => {
                    Thread.Sleep(2000); // allows session thread to load latest game data
                    return true;
                }, onSuccess: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        Session.GameWindow = new GameWindow(game);
                        Session.GameWindow.Show();
                        Close();
                    });
                }, onFinish: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        openEventArgs.Session.Close(true);
                    });
                }, false);
            }, null);
        }

        public void ShowLetterSelectionOptions(bool show) {
            if (show) {
                letterSelectionOptionsGrid.Visibility = Visibility.Visible;
                waitingForLetterSelectionGrid.Visibility = Visibility.Hidden;
            } else {
                letterSelectionOptionsGrid.Visibility = Visibility.Hidden;
                waitingForLetterSelectionGrid.Visibility = Visibility.Visible;
            }
        }

        private void SelectRandomLetterButton_Click(object sender, RoutedEventArgs e) {
            if (game.SetLetter(true, Session.Account.Id)) {
                inGameService.StartGame(game.ActiveGuidGame);
                return;
            }
            MessageBox.Show(Properties.Resources.CouldntSelectLetterErrorText);
        }

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

        private void SelectLetterButton_Click(object sender, RoutedEventArgs e) {
            if (letterComboBox.SelectedItem == null) {
                MessageBox.Show(Properties.Resources.MustSelectALetterErrorText);
                return;
            } else if (game.SetLetter(false, Session.Account.Id, letterComboBox.SelectedItem.ToString())) {
                Session.Account.Coins -= Session.ROUND_LETTER_SELECTION_COST;
                inGameService.StartGame(game.ActiveGuidGame);
                return;
            }
            MessageBox.Show(Properties.Resources.CouldntSelectLetterErrorText);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
