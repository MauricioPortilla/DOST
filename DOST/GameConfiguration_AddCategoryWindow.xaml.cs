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

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameConfiguration_AddCategoryWindow.xaml
    /// </summary>
    public partial class GameConfiguration_AddCategoryWindow : Window {
        public string CategoryName {
            get { return categoryNameTextBox.Text; }
        }
        public bool CategoryAdded = false;
        public GameConfiguration_AddCategoryWindow() {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(CategoryName)) {
                MessageBox.Show(Properties.Resources.NewCategoryNameEmptyErrorText);
                return;
            }
            CategoryAdded = true;
            Close();
        }
    }
}
