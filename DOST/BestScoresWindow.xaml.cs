using DOST.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DOST {
    /// <summary>
    /// Lógica de interacción para BestScoresWindow.xaml
    /// </summary>
    public partial class BestScoresWindow : Window {
        private ObservableCollection<UserScore> bestScoresList = new ObservableCollection<UserScore>();
        public ObservableCollection<UserScore> BestScoresList {
            get { return bestScoresList; }
        }

        public BestScoresWindow() {
            DataContext = this;
            InitializeComponent();
            LoadScoresList();
        }

        private void LoadScoresList() {
            EngineNetwork.EstablishChannel<ICuentaService>((service) => {
                var scoresList = service.GetBestScores();
                scoresList.ForEach((userScore) => {
                    bestScoresList.Add(new UserScore {
                        Ranking = userScore.Posicion,
                        Username = userScore.Usuario,
                        Score = userScore.Puntuacion
                    });
                });
                return true;
            });
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

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

        private void ReturnButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
