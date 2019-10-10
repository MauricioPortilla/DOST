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
        private static Cuenta cuenta;
        public static Cuenta Cuenta {
            get { return cuenta; }
            set { cuenta = value; }
        }
        private static LoginWindow login;
        public static LoginWindow Login {
            get { return login; }
            set { login = value; }
        }
        private static MainMenuWindow mainMenu;
        public static MainMenuWindow MainMenu {
            get { return mainMenu; }
            set { mainMenu = value; }
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
        private static readonly ObservableCollection<Partida> gamesList = new ObservableCollection<Partida>();
        public static ObservableCollection<Partida> GamesList {
            get { return gamesList; }
        }
        private static readonly ObservableCollection<GameConfigurationWindow.GameCategoryItem> categoriesList = new ObservableCollection<GameConfigurationWindow.GameCategoryItem>() {
            new GameConfigurationWindow.GameCategoryItem() { Name = "Nombre", GameCategory = new CategoriaPartida(0, null, "Nombre") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Apellido", GameCategory = new CategoriaPartida(0, null, "Apellido") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Color", GameCategory = new CategoriaPartida(0, null, "Color") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Animal", GameCategory = new CategoriaPartida(0, null, "Animal") },
            new GameConfigurationWindow.GameCategoryItem() { Name = "Fruta", GameCategory = new CategoriaPartida(0, null, "Fruta") }
        };
        public static ObservableCollection<GameConfigurationWindow.GameCategoryItem> CategoriesList {
            get { return categoriesList; }
        }

        public static void GetGamesList() {
            EngineNetwork.EstablishChannel<IPartidaService>((service) => {
                while (mainMenu != null) {
                    List<Services.Partida> serviceGamesList = service.GetPartidasList();
                    foreach (var serviceGame in serviceGamesList) {
                        List<Jugador> jugadores = new List<Jugador>();
                        Partida partida = new Partida(
                            serviceGame.Id,
                            serviceGame.Ronda,
                            serviceGame.Fecha,
                            jugadores
                        );
                        service.GetJugadoresList(serviceGame.Id).ForEach(
                            jugador => jugadores.Add(new Jugador(
                                jugador.Id,
                                new Cuenta(jugador.Cuenta.Id) {
                                    Username = jugador.Cuenta.Usuario,
                                    Password = jugador.Cuenta.Password,
                                    Email = jugador.Cuenta.Correo,
                                    Coins = jugador.Cuenta.Monedas,
                                    CreationDate = jugador.Cuenta.FechaCreacion,
                                    Verified = jugador.Cuenta.Confirmada,
                                    ValidationCode = jugador.Cuenta.CodigoValidacion
                                },
                                partida,
                                jugador.Puntuacion,
                                jugador.Anfitrion)
                            )
                        );
                        var gameCategories = new List<CategoriaPartida>();
                        service.GetCategoriasList(serviceGame.Id).ForEach(
                            categoria => gameCategories.Add(new CategoriaPartida(categoria.Id, partida, categoria.Nombre))
                        );
                        partida.Categorias = gameCategories;
                        partida.Jugadores = jugadores;
                        partida.PropertyChanged += Game_PropertyChanged;
                        if (gamesList.ToList().Exists(game => game.Id == serviceGame.Id)) {
                            var existentGame = gamesList.ToList().Find(game => game.Id == serviceGame.Id);
                            existentGame.Jugadores = jugadores;
                            existentGame.Categorias = gameCategories;
                            continue;
                        }
                        Application.Current.Dispatcher.Invoke(delegate {
                            gamesList.Add(partida);
                        });
                    }
                    List<Partida> gamesToRemove = new List<Partida>();
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
                mainMenu.gamesListView.Items.Refresh();
            });
        }
    }
}
