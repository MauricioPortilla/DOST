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
    /// Lógica de interacción para GameConfigurationWindow.xaml
    /// </summary>
    public partial class GameConfigurationWindow : Window {
        private ObservableCollection<GameCategory> categoriesList = new ObservableCollection<GameCategory>();
        public ObservableCollection<GameCategory> CategoriesList {
            get { return categoriesList; }
        }
        private Partida partida;

        public GameConfigurationWindow(ref Partida partida) {
            DataContext = this;
            InitializeComponent();
            this.partida = partida;
            partida.Categorias.ForEach(
                category => categoriesList.Add(new GameCategory {
                    IsSelected = true,
                    CategoriaPartida = category
                })
            );
            Session.CategoriesList.ToList().ForEach((category) => {
                if (!categoriesList.ToList().Exists(cat => cat.Nombre == category.Nombre)) {
                    categoriesList.Add(category);
                }
            });
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e) {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        public class GameCategory {
            public bool IsSelected { get; set; } = false;
            private string nombre;
            public string Nombre {
                get {
                    if (CategoriaPartida != null) {
                        return CategoriaPartida.Nombre;
                    }
                    return nombre;
                }
                set { nombre = value; }
            }
            public CategoriaPartida CategoriaPartida { get; set; }
        }
    }
}
