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
        private Game game;
        private Player player;
        private bool showLetterSelectionOptions = false;
        private InGameServiceClient inGameService;

        public GameWindow_LetterSelection(ref Game game, ref Player player, bool showLetterSelectionOptions) {
            InitializeComponent();
            this.game = game;
            this.player = player;
            this.showLetterSelectionOptions = showLetterSelectionOptions;
            Closed += (sender, e) => {
                inGameService.LeavePlayer(this.game.ActiveGuidGame, this.player.ActivePlayerGuid);
                Session.MainMenuWindow.Show();
            };

            try {
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(game));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException -> " + communicationException.Message);
                MessageBox.Show("Se perdió la conexión.");
                return;
            }
            ShowLetterSelectionOptions(showLetterSelectionOptions);
        }

        public class InGameCallback : InGameCallbackHandler {
            public InGameCallback(Game game) {
                this.game = game;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                throw new NotImplementedException();
            }

            public override void StartGame(string guidGame) {
                throw new NotImplementedException();
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

        }

        private void SelectSpecificLetterButton_Click(object sender, RoutedEventArgs e) {

        }
    }
}
