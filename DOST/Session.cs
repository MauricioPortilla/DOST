﻿using DOST.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.ServiceModel;

namespace DOST {
    /// <summary>
    /// Manages the current session in the application.
    /// </summary>
    public class Session {

        private static readonly Dictionary<string, string> languages = new Dictionary<string, string>() {
            { "Español", "es-MX" }, { "English", "en-US" }
        };
        /// <summary>
        /// Languages available as language name and its culture.
        /// </summary>
        public static Dictionary<string, string> LANGUAGES {
            get { return languages; }
        }
        public static readonly int MAX_PLAYERS_IN_GAME = 4;
        public static readonly int MAX_ROUNDS_PER_GAME = 5;
        public static readonly int ROUND_LETTER_SELECTION_COST = 20;
        public static readonly int ROUND_GET_WORD_COST = 100;
        public static readonly int ROUND_REDUCE_TIME_COST = 50;
        public static readonly int ROUND_REDUCE_TIME_SECONDS = 5;
        public static readonly int MAX_COINS_PER_GAME_WIN = 80;
        public static readonly int SECONDS_PER_ROUND = 40;
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
        /// <summary>
        /// All the default game categories for a game.
        /// </summary>
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
        /// <summary>
        /// All the default game categories name for a game.
        /// </summary>
        public static readonly List<string> DefaultCategoriesNameList = new List<string>() {
            "Nombre", "Name", "Apellido", "Last name", "Color", "Animal", "Fruta", "Fruit"
        };
        private static bool isPlayerInGame = false;
        public static bool IsPlayerInGame {
            get { return isPlayerInGame; }
            set { isPlayerInGame = value; }
        }

        /// <summary>
        /// Sets all the data from game that is being played in this session.
        /// </summary>
        /// <param name="service">GameService service</param>
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
                categories.Last().CategoryPlayerAnswer = new List<CategoryPlayerAnswer>();
                foreach (var categoryAnswer in category.CategoryPlayerAnswer) {
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

        /// <summary>
        /// Establishes a connection with game service to get all the games in course and waiting for players to be joined.
        /// </summary>
        public static void GetGamesList() {
            while (true) {
                bool valueReturned = EngineNetwork.EstablishChannel<IGameService>((service) => {
                    while (mainMenuWindow != null) {
                        if (IsPlayerInGame) {
                            GetGameBeingPlayedData(service);
                            continue;
                        }
                        List<Services.Game> serviceGamesList = service.GetGamesList();
                        foreach (Services.Game serviceGame in serviceGamesList) {
                            SetServiceGame(serviceGame);
                        }
                        RemoveUnnecesaryGames(serviceGamesList);
                    }
                    return true;
                });
                if (valueReturned) {
                    break;
                }
            }
        }

        /// <summary>
        /// Sets all the data to a Game object from a Service Game object and
        /// adds or replace it in the game list.
        /// </summary>
        /// <param name="serviceGame">Game from service</param>
        private static void SetServiceGame(Services.Game serviceGame) {
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
                return;
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

        /// <summary>
        /// Removes games from games list whose round is different than zero, or limit of players have been
        /// reached, or doesn't exist anymore.
        /// </summary>
        /// <param name="serviceGamesList">Games service list</param>
        private static void RemoveUnnecesaryGames(List<Services.Game> serviceGamesList) {
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

        /// <summary>
        /// Establishes a connection with chat callback to enter to the game chat.
        /// </summary>
        /// <param name="game">Game to interact</param>
        /// <param name="player">Player who will enter to the game chat</param>
        /// <param name="chatListBox">Chat list box where the messages will be placed on</param>
        /// <param name="chatServiceClient">Instance where connection will be placed on</param>
        public static void JoinGameChat(Game game, Player player, System.Windows.Controls.ListBox chatListBox, ref ChatServiceClient chatServiceClient) {
            InstanceContext chatInstanceContext = new InstanceContext(new ChatCallbackHandler(game, chatListBox));
            chatServiceClient = new ChatServiceClient(chatInstanceContext);
            while (true) {
                if (player != null) {
                    chatServiceClient.EnterChat(game.ActiveGuidGame, player.Account.Username);
                    break;
                }
            }
        }

        /// <summary>
        /// Gets executed when a game property changes and refreshes game list view of the main menu window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Game_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            Application.Current.Dispatcher.Invoke(delegate {
                mainMenuWindow.gamesListView.Items.Refresh();
            });
        }
    }
}
