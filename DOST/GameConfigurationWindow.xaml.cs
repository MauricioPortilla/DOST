using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DOST {

    /// <summary>
    /// Represents GameConfigurationWindow.xaml interaction logic.
    /// </summary>
    public partial class GameConfigurationWindow : Window {
        private ObservableCollection<GameCategoryItem> categoriesList = new ObservableCollection<GameCategoryItem>();
        public ObservableCollection<GameCategoryItem> CategoriesList {
            get { return categoriesList; }
        }
        private Game game;

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        /// <param name="game">Game to be configured</param>
        public GameConfigurationWindow(ref Game game) {
            DataContext = this;
            InitializeComponent();
            this.game = game;
            game.Categories.ForEach(
                category => categoriesList.Add(new GameCategoryItem {
                    IsSelected = true,
                    GameCategory = category
                })
            );
            var defaultCategories = Session.DefaultCategoriesList.ToList();
            defaultCategories.ForEach((category) => {
                if (!categoriesList.ToList().Exists(categoryInList => categoryInList.Name == category.Name)) {
                    categoriesList.Add(category);
                }
            });
        }

        /// <summary>
        /// Handles SaveButton click event.
        /// </summary>
        /// <param name="sender">SaveButton object</param>
        /// <param name="e">Button click event</param>
        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            if (!categoriesList.ToList().Exists(category => category.IsSelected)) {
                MessageBox.Show(Properties.Resources.MustSelectAtLeastOneCategoryErrorText);
                return;
            }
            bool didErrorHappen = false;
            categoriesList.ToList().ForEach((category) => {
                if ((game.Categories.Find(categoryInGame => categoryInGame.Name == category.Name) != null) && 
                    (!category.IsSelected && !game.RemoveCategory(category.GameCategory))
                ) {
                    didErrorHappen = true;
                } else if ((game.Categories.Find(categoryInGame => categoryInGame.Name == category.Name) == null) && 
                    category.IsSelected && !game.AddCategory(category.GameCategory)
                ) {
                    didErrorHappen = true;
                }
            });
            if (didErrorHappen) {
                MessageBox.Show(Properties.Resources.AnErrorHasOcurredErrorText);
                return;
            }
            MessageBox.Show(Properties.Resources.ChangesSavedText);
            Close();
        }

        /// <summary>
        /// Handles AddButton click event.
        /// </summary>
        /// <param name="sender">AddButton object</param>
        /// <param name="e">Button click event</param>
        private void AddButton_Click(object sender, RoutedEventArgs e) {
            var addCategoryWindow = new GameConfiguration_AddCategoryWindow();
            addCategoryWindow.Closed += (windowSender, windowEvent) => {
                if (!addCategoryWindow.CategoryAdded) {
                    return;
                }
                var newCategory = new GameCategory(0, game, addCategoryWindow.CategoryName);
                categoriesList.Add(new GameCategoryItem {
                    GameCategory = newCategory,
                    IsSelected = true,
                    Name = addCategoryWindow.CategoryName
                });
                categoriesItemsControl.Items.Refresh();
            };
            addCategoryWindow.Show();
        }

        /// <summary>
        /// Handles CancelButton click event.
        /// </summary>
        /// <param name="sender">CancelButton object</param>
        /// <param name="e">Button click event</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        /// <summary>
        /// Represents a game category in list.
        /// </summary>
        public class GameCategoryItem {
            public bool IsSelected { get; set; } = false;
            private string name;
            public string Name {
                get {
                    if (GameCategory != null) {
                        return GameCategory.Name;
                    }
                    return name;
                }
                set { name = value; }
            }
            public GameCategory GameCategory { get; set; }
        }
    }
}
