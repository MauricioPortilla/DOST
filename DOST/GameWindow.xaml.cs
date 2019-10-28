using DOST.Services;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Threading;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window {
        private Game game;
        private Player player;
        private InGameServiceClient inGameService;
        public static readonly int SECONDS_PER_ROUND = 40;
        private int timeRemaining = SECONDS_PER_ROUND;
        private List<StackPanel> categoriesStackPanels = new List<StackPanel>();
        private List<TextBox> categoriesTextBox = new List<TextBox>();
        private List<Button> categoriesButton = new List<Button>();
        private List<TextBlock> playersStatusTextBlock = new List<TextBlock>();

        public GameWindow(Game game) {
            InitializeComponent();
            this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
            this.player = this.game.Players.Find(playerInGame => playerInGame.Account.Id == Session.Account.Id);
            Title = Properties.Resources.RoundText + game.Round + " - DOST";
            roundTextBlock.Text = game.Round.ToString();
            try {
                InstanceContext gameInstance = new InstanceContext(new InGameCallback(
                    this.game, this.game.Players.Find(player => player.Account.Id == Session.Account.Id), categoriesTextBox, this
                ));
                inGameService = new InGameServiceClient(gameInstance);
                inGameService.EnterPlayer(game.ActiveGuidGame, player.ActivePlayerGuid);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameWindow) -> " + communicationException.Message);
                return;
            }
            LoadCategories();
            LoadPlayersStatus();
            LoadTimer();
        }

        private void LoadTimer() {
            Task.Run(() => {
                while (timeRemaining >= 0) {
                    Application.Current.Dispatcher.Invoke(delegate {
                        timeRemainingTextBlock.Text = timeRemaining.ToString();
                    });
                    timeRemaining--;
                    Thread.Sleep(1000);
                }
            });
        }

        private void LoadCategories() {
            Thickness textBlockMargin = new Thickness(30, 0, 0, 0);

            for (int index = 0; index < game.Categories.Count; index++) {
                playerAnswerCategoriesStackPanel.Children.Add(new TextBlock() {
                    Text = game.Categories[index].Name,
                    Margin = textBlockMargin,
                    Foreground = Brushes.White
                });
                categoriesStackPanels.Add(new StackPanel {
                    Margin = textBlockMargin,
                    Orientation = Orientation.Horizontal
                });
                categoriesTextBox.Add(new TextBox() {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 297,
                    Height = 38,
                    Margin = new Thickness(0, 0, 10, 10),
                    Foreground = Brushes.White,
                    Tag = index
                });
                categoriesTextBox.Last().KeyDown += CategoryTextBox_KeyDown;
                MaterialDesignThemes.Wpf.HintAssist.SetHint(categoriesTextBox[index], game.LetterSelected + "...");
                categoriesStackPanels[index].Children.Add(categoriesTextBox[index]);
                if (Session.CategoriesList.Exists(category => category.Name == game.Categories[index].Name)) {
                    categoriesButton.Add(new Button() {
                        Content = Properties.Resources.GetWordButton,
                        Margin = new Thickness(0, 0, 146, 0),
                        Tag = index
                    });
                    categoriesStackPanels[index].Children.Add(categoriesButton[index]);
                }
                playerAnswerCategoriesStackPanel.Children.Add(categoriesStackPanels[index]);
            }
        }

        private void CategoryTextBox_KeyDown(object sender, KeyEventArgs e) {
            var categoryTextBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(categoryTextBox.Text)) {
                if (e.Key != (Key) Enum.Parse(typeof(Key), game.LetterSelected)) {
                    e.Handled = true;
                    categoryTextBox.Text = "";
                }
            }
        }

        private void LoadPlayersStatus() {
            Thickness usernameMargin = new Thickness(20, 10, 0, 0);
            Thickness statusMargin = new Thickness(0, 10, 0, 0);
            for (int index = 0; index < game.Players.Count; index++) {
                var playerStackPanel = new StackPanel {
                    Orientation = Orientation.Horizontal
                };
                playerStackPanel.Children.Add(new TextBlock {
                    Text = game.Players[index].Account.Username,
                    Margin = usernameMargin,
                    Foreground = Brushes.White,
                    Width = 248,
                    Tag = index
                });
                playersStatusTextBlock.Add(new TextBlock {
                    Text = "DOST",
                    Margin = statusMargin,
                    Foreground = Brushes.White,
                    Opacity = 0.5,
                    Tag = index
                });
                playerStackPanel.Children.Add(playersStatusTextBlock.Last());
                playersStatusStackPanel.Children.Add(playerStackPanel);
            }
        }

        private void DostButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        }

        public class InGameCallback : InGameCallbackHandler {
            private Player player;
            private List<TextBox> categoriesTextBox;
            private GameWindow window;

            public InGameCallback(Game game, Player player, List<TextBox> categoriesTextBox, GameWindow window) {
                this.game = game;
                this.player = player;
                this.categoriesTextBox = categoriesTextBox;
                this.window = window;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                throw new NotImplementedException();
            }

            public override void StartGame(string guidGame) {
                throw new NotImplementedException();
            }

            public override void StartRound(string guidGame) {
                throw new NotImplementedException();
            }

            public override void EndRound(string guidGame) {
                if (guidGame != game.ActiveGuidGame) {
                    return;
                }
                window.SendCategoryAnswers();
            }
        }

        private void SendCategoryAnswers() {
            DialogHost.Show(loadingStackPanel, "GameWindow_WindowDialogHost", (openSender, openEventArgs) => {
                List<CategoryPlayerAnswer> categoryPlayerAnswers = new List<CategoryPlayerAnswer>();
                for (int index = 0; index < categoriesTextBox.Count; index++) {
                    categoryPlayerAnswers.Add(new CategoryPlayerAnswer(0, player, game.Categories[index], categoriesTextBox[index].Text, game.Round));
                }
                EngineNetwork.DoNetworkAction(onExecute: () => {
                    return player.SendCategoryAnswers(categoryPlayerAnswers);
                }, onSuccess: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        openEventArgs.Session.Close(true);
                        Session.GameWindow.Close();
                        Session.GameWindow = null;
                        new GameWindow_EndRound(game).Show();
                    });
                }, onFinish: null, true);
            }, null);
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }

    public abstract class InGameCallbackHandler : IInGameServiceCallback {
        protected Game game;
        public Game Game {
            get { return game; }
            set { game = value; }
        }

        public abstract void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);
        public abstract void StartRound(string guidGame);
        public abstract void StartGame(string guidGame);
        public abstract void EndRound(string guidGame);
    }
}
