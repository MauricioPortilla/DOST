using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
    /// Lógica de interacción para MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window {
        public bool IsClosed { get; private set; } = false;
        private ObservableCollection<Game> gamesList = Session.GamesList;
        public ObservableCollection<Game> GamesList {
            get { return gamesList; }
        }
        private bool didCreateGame = false;
        private int lastIdGameCreated = 0;

        public MainMenuWindow() {
            DataContext = this;
            InitializeComponent();
            usernameTextBlock.Text = Session.Account.Username;
            coinsTextBlock.Text = Session.Account.Coins.ToString();
            // rankTextBlock.Text = Session.Cuenta.GetRank();
            GamesList.CollectionChanged += GamesList_CollectionChanged;
            new Thread(Session.GetGamesList).Start();
            new Thread(JoinGameIfNeeded).Start();
        }

        void GamesList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            gamesListView.Items.Refresh();
        }

        private void JoinGameButton_Click(object sender, RoutedEventArgs e) {
            if (gamesListView.SelectedItem == null) {
                return;
            }
            var selectedGame = (Game) gamesListView.SelectedItem;
            if (Session.Account.JoinGame(selectedGame, false)) {
                Session.GameLobbyWindow = new GameLobbyWindow(ref selectedGame);
                Session.GameLobbyWindow.Show();
                Hide();
            }
        }

        private void BestScoresButton_Click(object sender, RoutedEventArgs e) {
            new BestScoresWindow().Show();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e) {
            Session.Account = null;
            Session.MainMenuWindow = null;
            Session.LoginWindow.Show();
            Close();
        }

        private void CreateGameButton_Click(object sender, RoutedEventArgs e) {
            int idgame = 0;
            if (!Session.Account.CreateGame(out idgame)) {
                MessageBox.Show(Properties.Resources.CouldntCreateGameErrorText);
                return;
            }
            didCreateGame = true;
            lastIdGameCreated = idgame;
        }

        private void JoinGameIfNeeded() {
            while (!IsClosed) {
                Application.Current.Dispatcher.Invoke(delegate {
                    if (!didCreateGame) {
                        return;
                    } else if (lastIdGameCreated == 0) {
                        return;
                    }
                    Game gameCreated = Session.GamesList.ToList().Find(game => game.Id == lastIdGameCreated);
                    if (gameCreated == null) {
                        return;
                    }
                    didCreateGame = false;
                    lastIdGameCreated = 0;
                    if (Session.Account.JoinGame(gameCreated, true)) {
                        Session.GameLobbyWindow = new GameLobbyWindow(ref gameCreated);
                        Session.GameLobbyWindow.Show();
                        new GameConfigurationWindow(ref gameCreated).Show();
                        Hide();
                    }
                });
            }
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
