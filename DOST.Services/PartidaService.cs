using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DOST.DataAccess;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class PartidaService : IPartidaService {
        public List<Partida> GetPartidasList() {
            List<Partida> gamesList = new List<Partida>();
            using (DostDatabase db = new DostDatabase()) {
                var gamesListDb = (from partida in db.Partida
                                   where partida.ronda == 0 &&
                                   (from jugador in db.Jugador
                                    where jugador.idpartida == partida.idpartida
                                    select jugador.idjugador).Count() < 4
                                   select partida).ToList();
                gamesListDb.ForEach(
                    game => gamesList.Add(new Partida(game.idpartida, game.ronda, game.fecha))
                );
            }
            return gamesList;
        }

        public List<Jugador> GetJugadoresList(int idpartida) {
            List<Jugador> playersList = new List<Jugador>();
            using (DostDatabase db = new DostDatabase()) {
                var playersListDb = (from jugador in db.Jugador
                                     where jugador.idpartida == idpartida
                                     select jugador).ToList();
                playersListDb.ForEach(
                    player => playersList.Add(new Jugador(
                        player.idjugador,
                        new Cuenta(
                            player.Cuenta.idcuenta, player.Cuenta.usuario, 
                            player.Cuenta.password, player.Cuenta.correo, 
                            player.Cuenta.monedas, player.Cuenta.fechaCreacion, 
                            player.Cuenta.confirmada == 1, player.Cuenta.codigoValidacion
                        ),
                        null,
                        player.puntuacion,
                        player.anfitrion == 1
                    ))
                );
            }
            return playersList;
        }

        public Jugador GetJugador(int idcuenta, int idpartida) {
            Jugador jugador = new Jugador();
            using (DostDatabase db = new DostDatabase()) {
                var jugadorDb = db.Jugador.ToList().Find(player => player.idpartida == idpartida && player.idcuenta == idcuenta);
                if (jugadorDb != null) {
                    jugador.Id = jugadorDb.idjugador;
                    jugador.Cuenta = new Cuenta(
                        jugadorDb.Cuenta.idcuenta, jugadorDb.Cuenta.usuario,
                        jugadorDb.Cuenta.password, jugadorDb.Cuenta.correo,
                        jugadorDb.Cuenta.monedas, jugadorDb.Cuenta.fechaCreacion,
                        jugadorDb.Cuenta.confirmada == 1, jugadorDb.Cuenta.codigoValidacion
                    );
                    jugador.Puntuacion = jugadorDb.puntuacion;
                    jugador.Anfitrion = jugadorDb.anfitrion == 1;
                    jugador.Partida = null;
                }
            }
            return jugador;
        }

        public bool AddJugador(int idcuenta, int idpartida, bool asAnfitrion) {
            using (DostDatabase db = new DostDatabase()) {
                db.Jugador.Add(new DataAccess.Jugador() {
                    idcuenta = idcuenta,
                    idpartida = idpartida,
                    puntuacion = 0,
                    anfitrion = asAnfitrion ? 1 : 0
                });
                return db.SaveChanges() != 0;
            }
        }

        public bool RemoveJugador(int idcuenta, int idpartida) {
            var player = GetJugador(idcuenta, idpartida);
            if (player.Id == 0) {
                return false;
            }
            using (DostDatabase db = new DostDatabase()) {
                var playerAnfitrion = db.Jugador.ToList().Find(
                    playerDb => playerDb.idjugador == player.Id && 
                                playerDb.idpartida == idpartida && 
                                playerDb.anfitrion == 1
                );
                if (playerAnfitrion != null) {
                    db.Jugador.Remove(playerAnfitrion);
                    if ((db.Jugador.Where(playerDb => playerDb.idpartida == idpartida).Count() - 1) <= 0) {
                        db.Partida.Remove(db.Partida.ToList().Find(game => game.idpartida == idpartida));
                    } else {
                        db.Jugador.First(playerDatabase => playerDatabase.idpartida == idpartida).anfitrion = 1;
                    }
                } else {
                    var playerNoAnfitrion = db.Jugador.ToList().Find(
                        playerDb => playerDb.idjugador == player.Id &&
                                    playerDb.idpartida == idpartida
                    );
                    if (playerNoAnfitrion != null) {
                        db.Jugador.Remove(playerNoAnfitrion);
                        if (db.Jugador.Where(playerDb => playerDb.idpartida == idpartida).Count() == 0) {
                            db.Partida.Remove(db.Partida.ToList().Find(game => game.idpartida == idpartida));
                        }
                    }
                }
                return db.SaveChanges() != 0;
            }
        }

        public bool CreatePartida(out int idpartida) {
            using (DostDatabase db = new DostDatabase()) {
                var newGame = new DataAccess.Partida() {
                    ronda = 0,
                    fecha = DateTime.Now
                };
                db.Partida.Add(newGame);
                if (db.SaveChanges() != 0) {
                    idpartida = newGame.idpartida;
                    Engine.CategoriesList.ForEach((category) => {
                        db.CategoriaPartida.Add(new DataAccess.CategoriaPartida() {
                            idpartida = newGame.idpartida,
                            nombre = category
                        });
                    });
                    db.SaveChanges();
                    return true;
                }
            }
            idpartida = 0;
            return false;
        }

        public List<CategoriaPartida> GetCategoriasList(int idpartida) {
            List<CategoriaPartida> categoriesList = new List<CategoriaPartida>();
            using (DostDatabase db = new DostDatabase()) {
                var categories = (from category in db.CategoriaPartida
                                  where category.idpartida == idpartida
                                  select category).ToList();
                categories.ForEach(category => categoriesList.Add(
                    new CategoriaPartida(category.idcategoria, null, category.nombre)
                ));
            }
            return categoriesList;
        }
    }
}
