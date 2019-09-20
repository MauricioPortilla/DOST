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
            get {
                return gamesList;
            }
        }

        public static void GetGamesList() {
            EngineNetwork.EstablishChannel<IPartidaService>((service) => {
                while (mainMenu != null) {
                    Thread.Sleep(200);
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
                        partida.Jugadores = jugadores;
                        partida.PropertyChanged += Game_PropertyChanged;
                        if (gamesList.ToList().Exists(game => game.Id == serviceGame.Id)) {
                            gamesList.ToList().Find(game => game.Id == serviceGame.Id).Jugadores = jugadores;
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
