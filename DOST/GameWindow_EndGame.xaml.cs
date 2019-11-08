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
    /// Lógica de interacción para GameWindow_EndGame.xaml
    /// </summary>
    public partial class GameWindow_EndGame : Window {
        public bool IsClosed { get; private set; } = false;
        private Game game;
        private Player player;
        private ChatServiceClient chatService;
        private List<TextBlock> lobbyPlayersUsernameTextBlocks;
        private List<TextBlock> lobbyPlayersScoreTextBlocks;
        private List<TextBlock> lobbyPlayersPlaceTextBlocks;
        private List<TextBlock> lobbyPlayersPlaceTitleTextBlocks;
        private SortedList<int, string> sortedScores = new SortedList<int, string>();

        public GameWindow_EndGame(Game game) {
            InitializeComponent();
            this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
            try {
                InstanceContext chatInstance = new InstanceContext(new ChatCallbackHandler(game, chatListBox));
                chatService = new ChatServiceClient(chatInstance);
                while (true) {
                    player = game.Players.Find(playerInGame => playerInGame.Account.Id == Session.Account.Id);
                    if (player != null) {
                        chatService.EnterChat(game.ActiveGuidGame, player.Account.Username);
                        break;
                    }
                }
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameLobbyWindow) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                Close();
                return;
            }
            lobbyPlayersUsernameTextBlocks = new List<TextBlock>() {
                playerOneUsernameTextBlock, playerTwoUsernameTextBlock,
                playerThreeUsernameTextBlock, playerFourUsernameTextBlock
            };
            lobbyPlayersScoreTextBlocks = new List<TextBlock>() {
                playerOneScoreTextBlock, playerTwoScoreTextBlock,
                playerThreeScoreTextBlock, playerFourScoreTextBlock
            };
            lobbyPlayersPlaceTextBlocks = new List<TextBlock>() {
                playerOnePlaceTextBlock, playerTwoPlaceTextBlock,
                playerThreePlaceTextBlock, playerFourPlaceTextBlock
            };
            lobbyPlayersPlaceTitleTextBlocks = new List<TextBlock>() {
                playerOnePlaceTitleTextBlock, playerTwoPlaceTitleTextBlock,
                playerThreePlaceTitleTextBlock, playerFourPlaceTitleTextBlock
            };
            LoadPlayers();
        }

        private void CalculatePlaces() {
            foreach (var playerInGame in game.Players) {
                sortedScores.Add(playerInGame.Score, playerInGame.ActivePlayerGuid);
            }
        }

        private void LoadPlayers() {
            var players = game.Players;
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++) {
                lobbyPlayersUsernameTextBlocks[playerIndex].Text = players[playerIndex].Account.Username;
                lobbyPlayersUsernameTextBlocks[playerIndex].Visibility = Visibility.Visible;
                lobbyPlayersPlaceTextBlocks[playerIndex].Text = "#" + sortedScores.IndexOfValue(players[playerIndex].ActivePlayerGuid);
                lobbyPlayersPlaceTextBlocks[playerIndex].Visibility = Visibility.Visible;
                lobbyPlayersScoreTextBlocks[playerIndex].Text += ": " + players[playerIndex].Score;
                lobbyPlayersScoreTextBlocks[playerIndex].Visibility = Visibility.Visible;
                lobbyPlayersPlaceTitleTextBlocks[playerIndex].Visibility = Visibility.Visible;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            if (player.LeaveGame(game)) {
                try {
                    chatService.LeaveChat(game.ActiveGuidGame, player.Account.Username);
                    Close();
                } catch (CommunicationException communicationException) {
                    Console.WriteLine("CommunicationException (ExitButton_Click) -> " + communicationException.Message);
                    MessageBox.Show(Properties.Resources.AnErrorHasOcurredErrorText);
                }
            }
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (string.IsNullOrWhiteSpace(chatMessageTextBox.Text)) {
                    return;
                }
                chatService.BroadcastMessage(game.ActiveGuidGame, player.Account.Username, chatMessageTextBox.Text);
                chatMessageTextBox.Clear();
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
            Session.IsPlayerInGame = false;
            Session.MainMenuWindow.Show();
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
