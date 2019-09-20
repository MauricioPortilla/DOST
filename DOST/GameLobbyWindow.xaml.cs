using DOST.Services;
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
    /// Lógica de interacción para GameLobbyWindow.xaml
    /// </summary>
    public partial class GameLobbyWindow : Window {
        public bool IsClosed { get; private set; } = false;
        private Partida partida;
        private Jugador jugador;
        private List<TextBlock> lobbyPlayersUsernameTextBlocks;
        private List<TextBlock> lobbyPlayersTypeTextBlocks;
        private List<TextBlock> lobbyPlayersRankTextBlocks;
        private List<TextBlock> lobbyPlayersRankTitleTextBlocks;
        private static readonly int MAX_NUMBER_OF_PLAYERS = 4;
        private ChatServiceClient chatService;

        public GameLobbyWindow(ref Partida partida) {
            InitializeComponent();
            this.partida = partida;
            lobbyPlayersUsernameTextBlocks = new List<TextBlock>() {
                playerOneUsernameTextBlock, playerTwoUsernameTextBlock,
                playerThreeUsernameTextBlock, playerFourUsernameTextBlock
            };
            lobbyPlayersTypeTextBlocks = new List<TextBlock>() {
                playerOneTypeTextBlock, playerTwoTypeTextBlock,
                playerThreeTypeTextBlock, playerFourTypeTextBlock
            };
            lobbyPlayersRankTextBlocks = new List<TextBlock>() {
                playerOneRankTextBlock, playerTwoRankTextBlock,
                playerThreeRankTextBlock, playerFourRankTextBlock
            };
            lobbyPlayersRankTitleTextBlocks = new List<TextBlock>() {
                playerOneRankTitleTextBlock, playerTwoRankTitleTextBlock,
                playerThreeRankTitleTextBlock, playerFourRankTitleTextBlock
            };
            Thread loadPlayersJoinedDataThread = new Thread(LoadPlayersJoinedData);
            loadPlayersJoinedDataThread.Start();
            InstanceContext chatInstance = new InstanceContext(new ChatCallbackHandler(partida, chatListBox));
            chatService = new ChatServiceClient(chatInstance);
            while (true) {
                jugador = partida.Jugadores.Find(player => player.Cuenta.Id == Session.Cuenta.Id);
                if (jugador != null) {
                    chatService.EnterChat(partida.Id, jugador.Cuenta.Usuario);
                    break;
                }
            }
        }

        public class ChatCallbackHandler : IChatServiceCallback {
            private Partida partida;
            private ListBox chatListBox;
            public ChatCallbackHandler(Partida partida, ListBox chatListBox) {
                this.partida = partida;
                this.chatListBox = chatListBox;
            }
            public void BroadcastMessage(int idpartida, string username, string message) {
                if (idpartida == partida.Id) {    
                    chatListBox.Items.Add(new TextBlock() {
                        Text = username + ": " + message
                    });
                    chatListBox.ScrollIntoView(chatListBox.Items[chatListBox.Items.Count - 1]);
                }
            }
        }

        public void LoadPlayersJoinedData() {
            while (!IsClosed) {
                Application.Current.Dispatcher.Invoke(delegate {
                    if (partida.Jugadores.Count == 0) {
                        return;
                    }
                    var anfitrion = partida.Jugadores.Find(x => x.Cuenta.Id == Session.Cuenta.Id);
                    if (anfitrion != null) {
                        if (!anfitrion.Anfitrion) {
                            startGameButton.Content = Properties.Resources.ReadyButton;
                        }
                    }
                    for (int index = 0; index < partida.Jugadores.Count; index++) {
                        if (lobbyPlayersUsernameTextBlocks[index].Text == partida.Jugadores[index].Cuenta.Usuario) {
                            continue;
                        }
                        lobbyPlayersUsernameTextBlocks[index].Text = partida.Jugadores[index].Cuenta.Usuario;
                        lobbyPlayersTypeTextBlocks[index].Text = partida.Jugadores[index].Anfitrion ?
                            Properties.Resources.HostPlayerText : Properties.Resources.PlayerText;
                        // lobbyPlayersRankTextBlocks[index].Text = partida.Jugadores[index].GetRank();
                        lobbyPlayersRankTextBlocks[index].Visibility = Visibility.Visible;
                        lobbyPlayersRankTitleTextBlocks[index].Visibility = Visibility.Visible;
                    }
                    var countPlayers = lobbyPlayersUsernameTextBlocks.Count(x => !string.IsNullOrWhiteSpace(x.Text));
                    if (partida.Jugadores.Count < countPlayers) {
                        for (int index = countPlayers; index < MAX_NUMBER_OF_PLAYERS; index++) {
                            lobbyPlayersUsernameTextBlocks[index].Text = "...";
                            lobbyPlayersTypeTextBlocks[index].Text = Properties.Resources.WaitingForPlayerText;
                            // lobbyPlayersRankTextBlocks[index].Text = "#0";
                            lobbyPlayersRankTextBlocks[index].Visibility = Visibility.Hidden;
                            lobbyPlayersRankTitleTextBlocks[index].Visibility = Visibility.Hidden;
                        }
                    }
                    if (partida.Jugadores.Count == MAX_NUMBER_OF_PLAYERS) {
                        lobbyStatusTextBlock.Text = "";
                    } else {
                        lobbyStatusTextBlock.Text = Properties.Resources.WaitingForPlayersText;
                    }
                });
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            if (Session.Cuenta.LeaveGame(partida)) {
                chatService.LeaveChat(partida.Id, jugador.Cuenta.Usuario);
                Session.GameLobbyWindow = null;
                Session.MainMenu.Show();
                Close();
            }
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e) {

        }

        private void ConfigurationButton_Click(object sender, RoutedEventArgs e) {

        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                chatService.BroadcastMessage(partida.Id, jugador.Cuenta.Usuario, chatMessageTextBox.Text);
                chatMessageTextBox.Clear();
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
