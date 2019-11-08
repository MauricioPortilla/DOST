using DOST.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
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
            scoreTextBlock.Text = player.Score.ToString();
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
            LoadPlayers();
            LoadTimer();
        }

        private void LoadTimer() {
            Task.Run(() => {
                bool isSoundPlaying = false;
                while (timeRemaining >= 0) {
                    if (timeRemaining == 10 && !isSoundPlaying) {
                        SoundPlayer soundPlayer = new SoundPlayer(Properties.SoundResources.HurrySFX as Stream);
                        //soundPlayer.Play();
                        isSoundPlaying = true;
                    }
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
                HintAssist.SetHint(categoriesTextBox[index], game.LetterSelected + "...");
                categoriesStackPanels[index].Children.Add(categoriesTextBox[index]);
                if (Session.DefaultCategoriesNameList.Contains(game.Categories[index].Name)) {
                    categoriesButton.Add(new Button() {
                        Content = Properties.Resources.GetWordButton,
                        Margin = new Thickness(0, 0, 10, 0),
                        Tag = index
                    });
                    categoriesButton.Last().Click += CategoryGetWordButton_Click;
                    categoriesStackPanels[index].Children.Add(categoriesButton.Last());
                    categoriesStackPanels[index].Children.Add(new TextBlock {
                        Text = Session.ROUND_GET_WORD_COST + " " + Properties.Resources.ScorePointsText,
                        Foreground = Brushes.White,
                        Margin = new Thickness(0, 15, 0, 0)
                    });
                }
                playerAnswerCategoriesStackPanel.Children.Add(categoriesStackPanels[index]);
            }
        }

        private void CategoryGetWordButton_Click(object sender, RoutedEventArgs e) {
            var getWordButton = sender as Button;
            if (player.Score < Session.ROUND_GET_WORD_COST) {
                MessageBox.Show(Properties.Resources.YouDontHaveEnoughScorePointsErrorText);
                return;
            }
            var word = player.GetCategoryWord(game.Categories[(int) getWordButton.Tag]);
            if (string.IsNullOrWhiteSpace(word)) {
                MessageBox.Show(Properties.Resources.WordNotFoundErrorText);
                return;
            }
            scoreTextBlock.Text = (Convert.ToInt32(scoreTextBlock.Text) - Session.ROUND_GET_WORD_COST).ToString();
            categoriesTextBox[(int) getWordButton.Tag].Text = word;
            categoriesTextBox[(int) getWordButton.Tag].IsEnabled = false;
            getWordButton.IsEnabled = false;
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

        private void LoadPlayers() {
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
            bool areAllCategoryTextBoxFilled = true;
            foreach (var categoryTextBox in categoriesTextBox) {
                if (categoryTextBox.Text == string.Empty) {
                    areAllCategoryTextBoxFilled = false;
                    break;
                }
            }
            if (!areAllCategoryTextBoxFilled) {
                MessageBox.Show(Properties.Resources.UncompletedFieldsErrorText);
                return;
            }
            try {
                inGameService.PressDost(game.ActiveGuidGame, player.ActivePlayerGuid);
                dostButton.IsEnabled = false;
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (DostButton_MouseLeftButtonDown) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.AnErrorHasOcurredErrorText);
                dostButton.IsEnabled = true;
            }
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
            }

            public override void StartGame(string guidGame) {
            }

            public override void StartRound(string guidGame, int playerSelectorIndex) {
            }

            public override void EndRound(string guidGame) {
                if (guidGame != game.ActiveGuidGame) {
                    return;
                }
                window.SendCategoryAnswers();
            }

            public override void PressDost(string guidGame, string guidPlayer) {
                if (game.ActiveGuidGame != guidGame) {
                    return;
                }
                var findPlayer = game.Players.Find(playerInGame => playerInGame.ActivePlayerGuid == guidPlayer);
                if (findPlayer == null) {
                    return;
                }
                try {
                    var playerStatus = window.playersStatusTextBlock[game.Players.IndexOf(findPlayer)];
                    playerStatus.Opacity = 1;
                    playerStatus.Foreground = Brushes.LimeGreen;
                } catch (ArgumentOutOfRangeException argumentOutOfRangeException) {
                    Console.WriteLine("ArgumentOutOfRangeException (PressDost in Callback) -> " + argumentOutOfRangeException.Message);
                    MessageBox.Show(Properties.Resources.AnErrorHasOcurredErrorText);
                }
            }
            public override void EndGame(string guidGame) {
            }
        }

        private void SendCategoryAnswers() {
            IsEnabled = false;
            DialogHost.Show(loadingStackPanel, "GameWindow_WindowDialogHost", (openSender, openEventArgs) => {
                List<CategoryPlayerAnswer> categoryPlayerAnswers = new List<CategoryPlayerAnswer>();
                for (int index = 0; index < categoriesTextBox.Count; index++) {
                    categoryPlayerAnswers.Add(new CategoryPlayerAnswer(0, player, game.Categories[index], categoriesTextBox[index].Text, game.Round));
                }
                EngineNetwork.DoNetworkAction(onExecute: () => {
                    return player.SendCategoryAnswers(categoryPlayerAnswers);
                }, onSuccess: () => {
                    Thread.Sleep(2500);
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
        public abstract void StartRound(string guidGame, int playerSelectorIndex);
        public abstract void StartGame(string guidGame);
        public abstract void EndRound(string guidGame);
        public abstract void PressDost(string guidGame, string guidPlayer);
        public abstract void EndGame(string guidGame);
    }
}
