using DOST.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace DOST {
    /// <summary>
    /// Represents BestScoresWindow.xaml interaction logic.
    /// </summary>
    public partial class BestScoresWindow : Window {
        private ObservableCollection<UserScore> bestScoresList = new ObservableCollection<UserScore>();
        public ObservableCollection<UserScore> BestScoresList {
            get { return bestScoresList; }
        }

        /// <summary>
        /// Creates a BestScoresWindow instance and initializes it.
        /// </summary>
        public BestScoresWindow() {
            DataContext = this;
            InitializeComponent();
            LoadScoresList();
        }

        /// <summary>
        /// Establishes a connection with account service to load the scores list.
        /// </summary>
        private void LoadScoresList() {
            EngineNetwork.EstablishChannel<IAccountService>((service) => {
                var scoresList = service.GetBestScores();
                scoresList.ForEach((userScore) => {
                    bestScoresList.Add(new UserScore {
                        Ranking = userScore.Ranking,
                        Username = userScore.Username,
                        Score = userScore.Score
                    });
                });
                return true;
            });
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
        /// Represents an account, its score, and its rank.
        /// </summary>
        public class UserScore {
            private int ranking;
            public int Ranking {
                get { return ranking; }
                set { ranking = value; }
            }
            private string username;
            public string Username {
                get { return username; }
                set { username = value; }
            }
            private int score;
            public int Score {
                get { return score; }
                set { score = value; }
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Click event handler</param>
        private void ReturnButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
