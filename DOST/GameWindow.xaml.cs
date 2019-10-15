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
    /// Lógica de interacción para GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window {
        private Game game;
        private InGameServiceClient inGameService;

        public GameWindow(ref Game game) {
            Title = Properties.Resources.RoundText + game.Round + " - DOST";
            InitializeComponent();
            this.game = game;
            InstanceContext gameInstance = new InstanceContext(new InGameCallbackHandler(game));
            inGameService = new InGameServiceClient(gameInstance);
            LoadCategories();
        }

        public void LoadCategories() {
            List<StackPanel> stackPanels = new List<StackPanel>();
            List<TextBox> categoriesTextBox = new List<TextBox>();
            List<Button> categoriesButton = new List<Button>();
            Thickness textBlocksMargin = new Thickness(30, 0, 0, 0);
            int categoryTextBoxWidth = 297;
            int categoryTextBoxHeight = 38;
            for (int index = 0; index < game.Categories.Count; index++) {
                playerAnswerCategoriesStackPanel.Children.Add(new TextBlock() {
                    Text = game.Categories[index].Name,
                    Margin = textBlocksMargin
                });
                stackPanels.Add(new StackPanel {
                    Margin = textBlocksMargin,
                    Orientation = Orientation.Horizontal
                });
                categoriesTextBox.Add(new TextBox() {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = categoryTextBoxWidth,
                    Height = categoryTextBoxHeight,
                    Margin = new Thickness(0, 0, 10, 10)
                });
                MaterialDesignThemes.Wpf.HintAssist.SetHint(categoriesTextBox[index], game.Categories[index].Name);
                stackPanels[index].Children.Add(categoriesTextBox[index]);
                categoriesButton.Add(new Button() {
                    Content = "Button",
                    Margin = new Thickness(0, 0, 146, 0)
                });
                stackPanels[index].Children.Add(categoriesButton[index]);
                playerAnswerCategoriesStackPanel.Children.Add(stackPanels[index]);
            }
        }

        public class InGameCallbackHandler : IInGameServiceCallback {
            private Game game;
            public Game Game {
                get { return game; }
                set { game = value; }
            }

            public InGameCallbackHandler(Game game) {
                this.game = game;
            }

            public void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady) {
                throw new NotImplementedException();
            }

            public void StartGame(string guidGame) {
                throw new NotImplementedException();
            }
        }


        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
