using System.Windows;

namespace DOST {

    /// <summary>
    /// Represents GameConfiguration_AddCategoryWindow.xaml interaction logic.
    /// </summary>
    public partial class GameConfiguration_AddCategoryWindow : Window {
        public string CategoryName {
            get { return categoryNameTextBox.Text; }
        }
        private bool categoryAdded = false;
        public bool CategoryAdded {
            get { return categoryAdded; }
            set { categoryAdded = value; }
        }

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        public GameConfiguration_AddCategoryWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles AddButton click event.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void AddButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(CategoryName)) {
                MessageBox.Show(Properties.Resources.NewCategoryNameEmptyErrorText);
                return;
            }
            categoryAdded = true;
            Close();
        }
    }
}
