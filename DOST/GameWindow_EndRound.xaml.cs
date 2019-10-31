using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ServiceModel;
using static DOST.GameLobbyWindow;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameWindow_EndRound.xaml
    /// </summary>
    public partial class GameWindow_EndRound : Window {
        private Game game;
        private Player player;
        private ChatServiceClient chatService;
        private InGameServiceClient inGameService;

        public GameWindow_EndRound(Game game) {
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
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(game, player));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameLobbyWindow) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                Close();
                return;
            }
            LoadCategoryPlayerAnswers();
        }

        public class InGameCallback : InGameCallbackHandler {
            private Player player;

            public InGameCallback(Game game, Player player) {
                this.game = game;
                this.player = player;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
            }

            public override void StartGame(string guidGame) {
            }

            public override void StartRound(string guidGame) {
            }

            public override void EndRound(string guidGame) {
            }

            public override void PressDost(string guidGame, string guidPlayer) {
            }
        }

        private void LoadCategoryPlayerAnswers() {
            for (int categoryIndex = 0; categoryIndex < game.Categories.Count; categoryIndex++) {
                var categoryStackPanel = new StackPanel {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(15)
                };
                categoryStackPanel.Children.Add(new TextBlock {
                    Text = game.Categories[categoryIndex].Name,
                    FontSize = 15,
                    Foreground = Brushes.White
                });
                for (int answerIndex = 0; answerIndex < game.Categories[categoryIndex].CategoryPlayerAnswer.Count; answerIndex++) {
                    var playerAnswer = game.Categories[categoryIndex].CategoryPlayerAnswer[answerIndex];
                    var playerAnswerStackPanel = new StackPanel {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    playerAnswerStackPanel.Children.Add(new TextBlock {
                        Text = playerAnswer.Player.Account.Username + ": ",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 5, 0),
                        Foreground = Brushes.White
                    });
                    playerAnswerStackPanel.Children.Add(new TextBlock {
                        Text = string.IsNullOrWhiteSpace(playerAnswer.Answer) ? "---" : playerAnswer.Answer,
                        Foreground = playerAnswer.HasCorrectAnswer ? Brushes.LightGreen : Brushes.Red
                    });
                    categoryStackPanel.Children.Add(playerAnswerStackPanel);
                }
                playerAnswersResultsStackPanel.Children.Add(categoryStackPanel);
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

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
