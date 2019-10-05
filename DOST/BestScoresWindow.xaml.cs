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
                        Posicion = userScore.Posicion,
                        Usuario = userScore.Usuario,
                        Puntuacion = userScore.Puntuacion
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
            private int posicion;
            public int Posicion {
                get { return posicion; }
                set { posicion = value; }
            }
            private string usuario;
            public string Usuario {
                get { return usuario; }
                set { usuario = value; }
            }
            private int puntuacion;
            public int Puntuacion {
                get { return puntuacion; }
                set { puntuacion = value; }
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
