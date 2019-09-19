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
        private ObservableCollection<Partida> gamesList = Session.GamesList;
        public ObservableCollection<Partida> GamesList {
            get {
                return gamesList;
            }
        }
        private bool didCreateGame = false;

        public MainMenuWindow() {
            DataContext = this;
            InitializeComponent();
            usernameTextBlock.Text = Session.Cuenta.Usuario;
            coinsTextBlock.Text = Session.Cuenta.Monedas.ToString();
            // rankTextBlock.Text = Session.Cuenta.GetRank();
            GamesList.CollectionChanged += GamesList_CollectionChanged;
            //Session.gameThreads.Start();
            new Thread(Session.GetGamesList).Start();
        }

        void GamesList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            gamesListView.Items.Refresh();
        }

        private void JoinGameButton_Click(object sender, RoutedEventArgs e) {
            if (gamesListView.SelectedItem == null) {
                return;
            }
            var selectedGame = (Partida) gamesListView.SelectedItem;
            if (Session.Cuenta.JoinGame(selectedGame)) {
                Session.GameLobbyWindow = new GameLobbyWindow(ref selectedGame);
                Session.GameLobbyWindow.Show();
                Hide();
            }
        }

        private void BestScoresButton_Click(object sender, RoutedEventArgs e) {

        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e) {
            Session.Cuenta.Logout();
            Session.Cuenta = null;
            Session.MainMenu = null;
            Session.Login.Show();
            Close();
        }

        private void CreateGameButton_Click(object sender, RoutedEventArgs e) {
            if (!Session.Cuenta.CreateGame()) {
                MessageBox.Show("Error al crear la partida");
                return;
            }
            didCreateGame = true;
        }

        public void JoinGameIfNeeded() {
            Application.Current.Dispatcher.Invoke(delegate {
                if (!didCreateGame) {
                    return;
                }
                Partida game = Session.GamesList.ToList().Find(
                    x => x.Jugadores.Find(j => j.Cuenta.Id == Session.Cuenta.Id) != null
                );
                if (game == null) {
                    return;
                }
                didCreateGame = false;
                Session.GameLobbyWindow = new GameLobbyWindow(ref game);
                Session.GameLobbyWindow.Show();
                Hide();
            });
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
