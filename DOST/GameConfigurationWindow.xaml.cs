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
        private ObservableCollection<GameCategoryItem> categoriesList = new ObservableCollection<GameCategoryItem>();
        public ObservableCollection<GameCategoryItem> CategoriesList {
            get { return categoriesList; }
        }
        private Game game;

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
            var defaultCategories = Session.CategoriesList.ToList();
            defaultCategories.ForEach((category) => {
                if (!categoriesList.ToList().Exists(categoryInList => categoryInList.Name == category.Name)) {
                    categoriesList.Add(category);
                }
            });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            if (!categoriesList.ToList().Exists(category => category.IsSelected == true)) {
                MessageBox.Show(Properties.Resources.MustSelectAtLeastOneCategoryErrorText);
                return;
            }
            categoriesList.ToList().ForEach((category) => {
                if (category.IsSelected) {
                    game.AddCategory(category.GameCategory);
                } else {
                    game.RemoveCategory(category.GameCategory);
                }
            });
            MessageBox.Show(Properties.Resources.ChangesSavedText);
            Close();
        }

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

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

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
