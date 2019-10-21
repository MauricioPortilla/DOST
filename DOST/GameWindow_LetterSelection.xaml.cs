using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
                Console.WriteLine("CommunicationException -> " + communicationException.Message);
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
                    Session.GameWindow = new GameWindow(game);
                    Session.GameWindow.Show();
                    window.Close();
                }
            }
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
            if (game.SetLetter(true)) {
                inGameService.StartGame(game.ActiveGuidGame);
                return;
            }
            MessageBox.Show(Properties.Resources.CouldntSelectLetterErrorText);
        }

        private void SelectSpecificLetterButton_Click(object sender, RoutedEventArgs e) {
            letterComboBox.Items.Clear();
            for (int letterASCII = 0; letterASCII <= 26; letterASCII++) {
                letterComboBox.Items.Add(Convert.ToChar(letterASCII).ToString());
            }
            letterSelectionOptionsGrid.Visibility = Visibility.Hidden;
            letterSelectionSelectorGrid.Visibility = Visibility.Visible;
        }

        private void SelectLetterButton_Click(object sender, RoutedEventArgs e) {
            if (letterComboBox.SelectedItem == null) {
                MessageBox.Show(Properties.Resources.MustSelectALetterErrorText);
                return;
            }
            if (game.SetLetter(false, letterComboBox.SelectedItem.ToString())) {
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
