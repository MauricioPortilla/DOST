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
        private static readonly ObservableCollection<Partida> gamesList = new ObservableCollection<Partida>();
        public static ObservableCollection<Partida> GamesList {
            get { return gamesList; }
        }
        private static readonly ObservableCollection<GameConfigurationWindow.GameCategory> categoriesList = new ObservableCollection<GameConfigurationWindow.GameCategory>() {
            new GameConfigurationWindow.GameCategory() { Nombre = "Nombre" },
            new GameConfigurationWindow.GameCategory() { Nombre = "Apellido" },
            new GameConfigurationWindow.GameCategory() { Nombre = "Color" },
            new GameConfigurationWindow.GameCategory() { Nombre = "Animal" },
            new GameConfigurationWindow.GameCategory() { Nombre = "Fruta" }
        };
        public static ObservableCollection<GameConfigurationWindow.GameCategory> CategoriesList {
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
                                    Usuario = jugador.Cuenta.Usuario,
                                    Password = jugador.Cuenta.Password,
                                    Correo = jugador.Cuenta.Correo,
                                    Monedas = jugador.Cuenta.Monedas,
                                    FechaCreacion = jugador.Cuenta.FechaCreacion,
                                    Confirmada = jugador.Cuenta.Confirmada,
                                    CodigoValidacion = jugador.Cuenta.CodigoValidacion
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
                    //Application.Current.Dispatcher.Invoke(delegate {
                    //    gamesToRemove.ForEach(x => gamesList.Remove(x));
                    //});
                    Thread.Sleep(200);
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
