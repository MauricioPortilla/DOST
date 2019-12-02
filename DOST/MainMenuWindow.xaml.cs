using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            IsVisibleChanged += (object sender, DependencyPropertyChangedEventArgs e) => {
                if ((bool) e.NewValue) {
                    LoadAccountDataUI();
                }
            };
        }

        /// <summary>
        /// Handles DialogHost loaded event. Tries to execute get games list method until it gets successful.
        /// </summary>
        /// <param name="sender">DialogHost object</param>
        /// <param name="e">DialogHost event</param>
        private void DialogHost_Loaded(object sender, RoutedEventArgs e) {
            IsEnabled = false;
            var dialog = DialogHost.Show(loadingStackPanel, "MainMenuWindow_WindowDialogHost", (openSender, openEventArgs) => {
                EngineNetwork.DoNetworkOperation(onExecute: () => {
                    var getGamesListTask = Task.Run(() => {
                        Session.GetGamesList();
                    });
                    Thread.Sleep(1000);
                    if (getGamesListTask.Status != TaskStatus.Running) {
                        return false;
                    }
                    new Thread(JoinGameIfNeeded).Start();
                    return true;
                }, onSuccess: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        openEventArgs.Session.Close(true);
                        IsEnabled = true;
                    });
                }, null, null, true);
            }, null);
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
            joinGameButton.IsEnabled = false;
            var selectedGame = (Game) gamesListView.SelectedItem;
            if (Session.Account.JoinGame(selectedGame, false, out string guidPlayer)) {
                Session.GameLobbyWindow = new GameLobbyWindow(ref selectedGame);
                Session.GameLobbyWindow.Show();
                Hide();
            } else {
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
            }
            joinGameButton.IsEnabled = true;
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
            createGameButton.IsEnabled = false;
            if (!Session.Account.CreateGame(out guidGame)) {
                MessageBox.Show(Properties.Resources.CouldntCreateGameErrorText);
                createGameButton.IsEnabled = true;
                return;
            }
            createGameButton.IsEnabled = true;
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
                    if (Session.Account.JoinGame(gameCreated, true, out string guidPlayer)) {
                        Session.GameLobbyWindow = new GameLobbyWindow(ref gameCreated);
                        Session.GameLobbyWindow.Show();
                        createGameButton.IsEnabled = true;
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
