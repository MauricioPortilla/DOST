using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

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
        private string lastGuidGameCreated = "";

        public MainMenuWindow() {
            DataContext = this;
            InitializeComponent();
            usernameTextBlock.Text = Session.Account.Username;
            coinsTextBlock.Text = Session.Account.Coins.ToString();
            rankTextBlock.Text = Session.Account.GetRank();
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
            string guidGame = "";
            if (Session.AllGamesAvailable.Find(game => game.Players.Find(player => player.Account.Id == Session.Account.Id) != null) != null) {
                MessageBox.Show(Properties.Resources.YouAreAlreadyInAGame);
                return;
            }
            if (!Session.Account.CreateGame(out guidGame)) {
                MessageBox.Show(Properties.Resources.CouldntCreateGameErrorText);
                return;
            }
            didCreateGame = true;
            lastGuidGameCreated = guidGame;
        }

        private void JoinGameIfNeeded() {
            while (!IsClosed) {
                Application.Current.Dispatcher.Invoke(delegate {
                    if (!didCreateGame) {
                        return;
                    } else if (lastGuidGameCreated == "") {
                        return;
                    }
                    Game gameCreated = Session.GamesList.ToList().Find(game => game.ActiveGuidGame == lastGuidGameCreated);
                    if (gameCreated == null) {
                        return;
                    }
                    didCreateGame = false;
                    lastGuidGameCreated = "";
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
