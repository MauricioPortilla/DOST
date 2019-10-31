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
using KellermanSoftware.CompareNetObjects;

namespace DOST {
    class Session {
        public static readonly Dictionary<string, string> LANGUAGES = new Dictionary<string, string>() {
            { "Español", "es-MX" }, { "English", "en-US" }
        };
        public static readonly int MAX_PLAYERS_IN_GAME = 4;
        public static readonly int ROUND_LETTER_SELECTION_COST = 20;
        public static readonly int ROUND_GET_WORD_COST = 100;
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
        private static List<Game> allGamesAvailable = new List<Game>();
        public static List<Game> AllGamesAvailable {
            get { return allGamesAvailable; }
            set { allGamesAvailable = value; }
        }
        private static readonly List<GameConfigurationWindow.GameCategoryItem> defaultCategoriesList = new List<GameConfigurationWindow.GameCategoryItem>() {
            new GameConfigurationWindow.GameCategoryItem() { Name = Properties.Resources.NameCategoryText, GameCategory = new GameCategory(0, null, Properties.Resources.NameCategoryText), IsSelected = false },
            new GameConfigurationWindow.GameCategoryItem() { Name = Properties.Resources.LastNameCategoryText, GameCategory = new GameCategory(0, null, Properties.Resources.LastNameCategoryText), IsSelected = false },
            new GameConfigurationWindow.GameCategoryItem() { Name = Properties.Resources.ColorCategoryText, GameCategory = new GameCategory(0, null, Properties.Resources.ColorCategoryText), IsSelected = false },
            new GameConfigurationWindow.GameCategoryItem() { Name = Properties.Resources.AnimalCategoryText, GameCategory = new GameCategory(0, null, Properties.Resources.AnimalCategoryText), IsSelected = false },
            new GameConfigurationWindow.GameCategoryItem() { Name = Properties.Resources.FruitCategoryText, GameCategory = new GameCategory(0, null, Properties.Resources.FruitCategoryText), IsSelected = false }
        };
        public static List<GameConfigurationWindow.GameCategoryItem> DefaultCategoriesList {
            get { return defaultCategoriesList; }
        }
        public static readonly List<string> DefaultCategoriesNameList = new List<string>() {
            "Nombre", "Name", "Apellido", "Last name", "Color", "Animal", "Fruta", "Fruit"
        };
        public static bool IsPlayerInGame = false;

        private static void GetGameBeingPlayedData(IGameService service) {
            var gameBeingPlayed = allGamesAvailable.Find(game => game.Players.Find(player => player.Account.Id == Session.Account.Id) != null);
            if (gameBeingPlayed == null) {
                return;
            }
            var activeGame = service.GetActiveGame(gameBeingPlayed.ActiveGuidGame);
            if (string.IsNullOrEmpty(activeGame.ActiveGameGuid)) {
                return;
            }
            var players = new List<Player>();
            activeGame.Players.ForEach((player) => {
                players.Add(new Player(
                    id: 0,
                    new Account(player.Account.Id) {
                        Username = player.Account.Username,
                        Password = player.Account.Password,
                        Email = player.Account.Email,
                        Coins = player.Account.Coins,
                        CreationDate = player.Account.CreationDate,
                        IsVerified = player.Account.IsVerified,
                        ValidationCode = player.Account.ValidationCode
                    },
                    gameBeingPlayed,
                    player.Score,
                    player.IsHost
                ) {
                    ActivePlayerGuid = player.ActivePlayerGuid,
                    IsReady = player.IsReady
                });
            });
            var categories = new List<GameCategory>();
            activeGame.GameCategories.ForEach((category) => {
                categories.Add(new GameCategory(0, gameBeingPlayed, category.Name));
                foreach (var categoryAnswer in category.CategoryPlayerAnswer) {
                    categories.Last().CategoryPlayerAnswer = new List<CategoryPlayerAnswer>();
                    categories.Last().CategoryPlayerAnswer.Add(new CategoryPlayerAnswer(0, null, categories.Last(), categoryAnswer.Answer, categoryAnswer.Round));
                    categories.Last().CategoryPlayerAnswer.Last().Player = players.Find(player => player.ActivePlayerGuid == categoryAnswer.Player.ActivePlayerGuid);
                    categories.Last().CategoryPlayerAnswer.Last().HasCorrectAnswer = categoryAnswer.HasCorrectAnswer;
                }
            });
            gameBeingPlayed.Round = activeGame.Round;
            gameBeingPlayed.RoundStartingTime = activeGame.RoundStartingTime;
            gameBeingPlayed.Categories = categories;
            gameBeingPlayed.Players = players;
            gameBeingPlayed.LetterSelected = activeGame.LetterSelected;
        }

        public static void GetGamesList() {
            EngineNetwork.EstablishChannel<IGameService>((service) => {
                while (mainMenuWindow != null) {
                    if (IsPlayerInGame) {
                        GetGameBeingPlayedData(service);
                        continue;
                    }
                    List<Services.Game> serviceGamesList = service.GetGamesList();
                    foreach (var serviceGame in serviceGamesList) {
                        List<Player> playersList = new List<Player>();
                        Game game = new Game(
                            serviceGame.Id,
                            serviceGame.Round,
                            serviceGame.Date,
                            playersList
                        ) {
                            ActiveGuidGame = serviceGame.ActiveGameGuid,
                            LetterSelected = serviceGame.LetterSelected,
                            RoundStartingTime = serviceGame.RoundStartingTime
                        };
                        serviceGame.Players.ForEach((player) => {
                            playersList.Add(new Player(
                                id: 0,
                                new Account(player.Account.Id) {
                                    Username = player.Account.Username,
                                    Password = player.Account.Password,
                                    Email = player.Account.Email,
                                    Coins = player.Account.Coins,
                                    CreationDate = player.Account.CreationDate,
                                    IsVerified = player.Account.IsVerified,
                                    ValidationCode = player.Account.ValidationCode
                                },
                                game,
                                player.Score,
                                player.IsHost
                            ) {
                                ActivePlayerGuid = player.ActivePlayerGuid,
                                IsReady = player.IsReady
                            });
                        });
                        var gameCategories = new List<GameCategory>();
                        serviceGame.GameCategories.ForEach((category) => {
                            gameCategories.Add(new GameCategory(0, game, category.Name));
                        });
                        game.Categories = gameCategories;
                        game.Players = playersList;
                        game.PropertyChanged += Game_PropertyChanged;
                        if (gamesList.ToList().Exists(findGame => findGame.ActiveGuidGame == serviceGame.ActiveGameGuid)) {
                            var existentGame = gamesList.ToList().Find(findGame => findGame.ActiveGuidGame == serviceGame.ActiveGameGuid);
                            existentGame.Players = playersList;
                            existentGame.Categories = gameCategories;
                            existentGame.LetterSelected = serviceGame.LetterSelected;
                            continue;
                        }
                        if (allGamesAvailable.Exists(findGame => findGame.ActiveGuidGame == serviceGame.ActiveGameGuid)) {
                            var existentGame = allGamesAvailable.ToList().Find(findGame => findGame.ActiveGuidGame == serviceGame.ActiveGameGuid);
                            existentGame.Players = playersList;
                            existentGame.Categories = gameCategories;
                            existentGame.LetterSelected = serviceGame.LetterSelected;
                            existentGame.RoundStartingTime = serviceGame.RoundStartingTime;
                            existentGame.Round = serviceGame.Round;
                        } else {
                            allGamesAvailable.Add(game);
                        }
                        Application.Current.Dispatcher.Invoke(delegate {
                            gamesList.Add(game);
                        });
                    }
                    List<Game> gamesToRemove = new List<Game>();
                    foreach (var game in gamesList) {
                        var getGame = serviceGamesList.Find(gameInServiceList => gameInServiceList.ActiveGameGuid == game.ActiveGuidGame);
                        if (getGame == null) {
                            gamesToRemove.Add(game);
                            allGamesAvailable.Remove(allGamesAvailable.Find(gameAvailable => gameAvailable.ActiveGuidGame == game.ActiveGuidGame));
                        } else if (getGame.Round != 0 || getGame.Players.Count >= MAX_PLAYERS_IN_GAME) {
                            gamesToRemove.Add(game);
                        }
                    }
                    gamesToRemove.ForEach((game) => {
                        Application.Current.Dispatcher.Invoke(delegate {
                            gamesList.Remove(game);
                        });
                    });
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
