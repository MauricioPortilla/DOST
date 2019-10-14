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
using static DOST.GameLobbyWindow;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window {
        private Game game;
        private InGameServiceClient inGameService;

        public GameWindow(ref Game game) {
            Title = Properties.Resources.RoundText + game.Round + " - DOST";
            InitializeComponent();
            this.game = game;
            InstanceContext gameInstance = new InstanceContext(new InGameCallbackHandler(game));
            inGameService = new InGameServiceClient(gameInstance);
        }

        public class InGameCallbackHandler : IInGameServiceCallback {
            private Game game;
            public Game Game {
                get { return game; }
                set { game = value; }
            }

            public InGameCallbackHandler(Game game) {
                this.game = game;
            }

            public void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                throw new NotImplementedException();
            }

            public void StartGame(string guidGame) {
                throw new NotImplementedException();
            }
        }


        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
