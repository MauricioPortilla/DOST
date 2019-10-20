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
using static DOST.GameLobbyWindow;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window {
        private Game game;
        private InGameServiceClient inGameService;
        public static readonly int SECONDS_FOR_ROUND = 40;
        
        public GameWindow(ref Game game) {
            Title = Properties.Resources.RoundText + game.Round + " - DOST";
            InitializeComponent();
            this.game = game;
            InstanceContext gameInstance = new InstanceContext(new InGameCallback(game));
            inGameService = new InGameServiceClient(gameInstance);
            LoadCategories();
            LoadPlayersStatus();

        }

        public void LoadCategories() {
            List<StackPanel> categoriesStackPanels = new List<StackPanel>();
            List<TextBox> categoriesTextBox = new List<TextBox>();
            List<Button> categoriesButton = new List<Button>();
            Thickness textBlocksMargin = new Thickness(30, 0, 0, 0);

            for (int index = 0; index < game.Categories.Count; index++) {
                playerAnswerCategoriesStackPanel.Children.Add(new TextBlock() {
                    Text = game.Categories[index].Name,
                    Margin = textBlocksMargin,
                    Foreground = Brushes.White
                });
                categoriesStackPanels.Add(new StackPanel {
                    Margin = textBlocksMargin,
                    Orientation = Orientation.Horizontal
                });
                categoriesTextBox.Add(new TextBox() {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 297,
                    Height = 38,
                    Margin = new Thickness(0, 0, 10, 10),
                    Foreground = Brushes.White
                });
                MaterialDesignThemes.Wpf.HintAssist.SetHint(categoriesTextBox[index], game.Categories[index].Name);
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

        public void LoadPlayersStatus() {
            List<StackPanel> statusStackPanels = new List<StackPanel>();
            List<TextBox> usernamesTextBox = new List<TextBox>();
            List<TextBox> statusTextBox = new List<TextBox>();
            Thickness textBlocksMargin = new Thickness(10, 0, 0, 0);
            for (int index = 0; index < game.Players.Count(); index++) {
                playerStatusStackPanel.Children.Add(new TextBlock() {
                    Text = game.Players[index].Account.Username,
                    Margin = textBlocksMargin,
                    Foreground = Brushes.White
                });
                statusStackPanels.Add(new StackPanel {
                    Margin = textBlocksMargin,
                    Orientation = Orientation.Horizontal
                });
                usernamesTextBox.Add(new TextBox(){
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 150,
                    Height = 30,
                    Margin = new Thickness(10, 10, 160, 0),
                    Foreground = Brushes.White
                });
                statusTextBox.Add(new TextBox() {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 45,
                    Height = 30,
                    Margin = new Thickness(10, 10, 160, 0),
                    Foreground = Brushes.White
                });
                
            }

        }

             public class InGameCallback : InGameCallbackHandler {
            public InGameCallback(Game game) {
                this.game = game;
            }

            public override void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                throw new NotImplementedException();
            }

            public override void StartGame(string guidGame) {
                throw new NotImplementedException();
            }
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
        public abstract void StartGame(string guidGame);
    }
}
