using DOST.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DOST {
    class Session {
        public static readonly Dictionary<string, string> LANGUAGES = new Dictionary<string, string>() {
            { "Español", "es-MX" }, { "English", "en-US" }
        };
        private static Account account;
        public static Account Account {
            get { return account; }
            set { account = value; }
        }
        private static LoginWindow loginWindow;
        public static LoginWindow LoginWindow {
            get { return loginWindow; }
            set { loginWindow = value; }
        }
        private static MainMenuWindow mainMenuWindow;
        public static MainMenuWindow MainMenuWindow {
            get { return mainMenuWindow; }
            set { mainMenuWindow = value; }
        }
        private static GameLobbyWindow gameLobbyWindow;
        public static GameLobbyWindow GameLobbyWindow {
            get { return gameLobbyWindow; }
            set { gameLobbyWindow = value; }
        }
        private static GameWindow gameWindow;
        public static GameWindow GameWindow {
            get { return gameWindow; }
            set { gameWindow = value; }
        }
        private static readonly ObservableCollection<Game> gamesList = new ObservableCollection<Game>();
        public static ObservableCollection<Game> GamesList {
            get { return gamesList; }
        }
        private static readonly ObservableCollection<GameConfigurationWindow.GameCategoryItem> categoriesList = new ObservableCollection<GameConfigurationWindow.GameCategoryItem>() {
            new GameConfigurationWindow.GameCategoryItem() { Name = "Nombre", GameCategory = new GameCategory(0, null, "Nombre") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Apellido", GameCategory = new GameCategory(0, null, "Apellido") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Color", GameCategory = new GameCategory(0, null, "Color") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Animal", GameCategory = new GameCategory(0, null, "Animal") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Fruta", GameCategory = new GameCategory(0, null, "Fruta") }
        };
        public static ObservableCollection<GameConfigurationWindow.GameCategoryItem> CategoriesList {
            get { return categoriesList; }
        }

        public static void GetGamesList() {
            EngineNetwork.EstablishChannel<IGameService>((service) => {
                while (mainMenuWindow != null) {
                    List<Services.Game> serviceGamesList = service.GetGamesList();
                    foreach (var serviceGame in serviceGamesList) {
                        List<Player> playersList = new List<Player>();
                        Game game = new Game(
                            serviceGame.Id,
                            serviceGame.Round,
                            serviceGame.Date,
                            playersList
                        );
                        service.GetPlayersList(serviceGame.Id).ForEach(
                            playerService => playersList.Add(new Player(
                                playerService.Id,
                                new Account(playerService.Account.Id) {
                                    Username = playerService.Account.Username,
                                    Password = playerService.Account.Password,
                                    Email = playerService.Account.Email,
                                    Coins = playerService.Account.Coins,
                                    CreationDate = playerService.Account.CreationDate,
                                    IsVerified = playerService.Account.IsVerified,
                                    ValidationCode = playerService.Account.ValidationCode
                                },
                                game,
                                playerService.Score,
                                playerService.IsHost)
                            )
                        );
                        var gameCategories = new List<GameCategory>();
                        service.GetCategoriesList(serviceGame.Id).ForEach(
                            categoria => gameCategories.Add(new GameCategory(categoria.Id, game, categoria.Name))
                        );
                        game.Categories = gameCategories;
                        game.Players = playersList;
                        game.PropertyChanged += Game_PropertyChanged;
                        if (gamesList.ToList().Exists(findGame => findGame.Id == serviceGame.Id)) {
                            var existentGame = gamesList.ToList().Find(findGame => findGame.Id == serviceGame.Id);
                            existentGame.Players = playersList;
                            existentGame.Categories = gameCategories;
                            continue;
                        }
                        Application.Current.Dispatcher.Invoke(delegate {
                            gamesList.Add(game);
                        });
                    }
                    List<Game> gamesToRemove = new List<Game>();
                    foreach (var game in gamesList) {
                        var getGame = serviceGamesList.Find(gameInServiceList => gameInServiceList.Id == game.Id);
                        if (getGame == null) {
                            gamesToRemove.Add(game);
                        }
                    }
                    gamesToRemove.ForEach((game) => {
                        Application.Current.Dispatcher.Invoke(delegate {
                            gamesList.Remove(game);
                        });
                    });
                    Thread.Sleep(400);
                }
                return true;
            });
        }

        static void Game_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            Application.Current.Dispatcher.Invoke(delegate {
                mainMenuWindow.gamesListView.Items.Refresh();
            });
        }
    }
}
