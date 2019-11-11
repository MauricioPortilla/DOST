using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace DOST {
    /// <summary>
    /// Represents MainMenuWindow.xaml interaction logic.
    /// </summary>
    public partial class MainMenuWindow : Window {
        public bool IsClosed { get; private set; } = false;
        private ObservableCollection<Game> gamesList = Session.GamesList;
        public ObservableCollection<Game> GamesList {
            get { return gamesList; }
        }
        private bool didCreateGame = false;
        private string lastGuidGameCreated = "";

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        public MainMenuWindow() {
            DataContext = this;
            InitializeComponent();
            GamesList.CollectionChanged += GamesList_CollectionChanged;
            new Thread(Session.GetGamesList).Start();
            new Thread(JoinGameIfNeeded).Start();
            IsVisibleChanged += (object sender, DependencyPropertyChangedEventArgs e) => {
                if ((bool) e.NewValue) {
                    LoadAccountDataUI();
                }
            };
        }

        /// <summary>
        /// Reloads account data from server and changes it on UI.
        /// </summary>
        private void LoadAccountDataUI() {
            Session.Account.Reload();
            usernameTextBlock.Text = Session.Account.Username;
            coinsTextBlock.Text = Session.Account.Coins.ToString();
            rankTextBlock.Text = Session.Account.GetRank();
        }

        /// <summary>
        /// Handles GameList collection changed event. If any item in collection changes, the list view refreshes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GamesList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            gamesListView.Items.Refresh();
        }

        /// <summary>
        /// Handles JoinGameButton click event. Establishes a connection with game service to try to
        /// join to the game selected in the list view.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
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

        /// <summary>
        /// Handles BestScoresButton click event. Shows a new BestScoresWindow.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void BestScoresButton_Click(object sender, RoutedEventArgs e) {
            new BestScoresWindow().Show();
        }

        /// <summary>
        /// Handles LogoutButton click event. Closes actual window and shows up the login window.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void LogoutButton_Click(object sender, RoutedEventArgs e) {
            Session.Account = null;
            Session.MainMenuWindow = null;
            Session.LoginWindow.Show();
            Close();
        }

        /// <summary>
        /// Handles CreateGameButton click event. Establishes a connection with game service to create a new game.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
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

        /// <summary>
        /// Verifies if a new game was created by session player to join him to the new game.
        /// </summary>
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
        /// Closes actual window.
        /// </summary>
        /// <param name="e">Window event</param>
        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
