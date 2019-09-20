using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class PartidaService : IPartidaService {
        public List<Partida> GetPartidasList() {
            //Database.InitializeDatabase();
            List<Partida> gamesList = new List<Partida>();
            Database.ExecuteStoreQuery(
                "SELECT * FROM partida p WHERE ronda = 0 AND (" +
                    "SELECT COUNT(idjugador) AS numPlayers FROM jugador WHERE idpartida = p.idpartida" +
                ") < 4", null, (results) => {
                    foreach (var row in results) {
                        gamesList.Add(new Partida(
                            (int) row["idpartida"],
                            (int) row["ronda"],
                            DateTime.Parse(row["fecha"].ToString())
                        ));
                    }
                }
            );
            return gamesList;
        }

        public List<Jugador> GetJugadoresList(int idpartida) {
            //Database.InitializeDatabase();
            List<Jugador> playersList = new List<Jugador>();
            Database.ExecuteStoreQuery(
                "SELECT * FROM jugador WHERE idpartida = @idpartida",
                new Dictionary<string, object>() {
                    { "@idpartida", idpartida }
                }, (results) => {
                    foreach (var row in results) {
                        playersList.Add(new Jugador(
                            (int) row["idjugador"],
                            new Cuenta((int) row["idcuenta"]),
                            null,
                            (int) row["puntuacion"],
                            (int) row["anfitrion"] == 1
                        ));
                    }
                }
            );
            return playersList;
        }

        public Jugador GetJugador(int idcuenta, int idpartida) {
            Jugador jugador = new Jugador();
            Database.ExecuteStoreQuery(
                "SELECT * FROM jugador WHERE idpartida = @idpartida AND idcuenta = @idcuenta",
                new Dictionary<string, object>() {
                    { "@idpartida", idpartida }, { "@idcuenta", idcuenta }
                }, (results) => {
                    var row = results[0];
                    jugador.Id = (int) row["idjugador"];
                    jugador.Cuenta = new Cuenta((int) row["idcuenta"]);
                    jugador.Puntuacion = (int) row["puntuacion"];
                    jugador.Anfitrion = (int) row["anfitrion"] == 1;
                }
            );
            return jugador;
        }

        public bool AddJugador(int idcuenta, int idpartida, bool asAnfitrion) {
            return Database.ExecuteUpdate(
                "INSERT INTO jugador VALUES (@idcuenta, @idpartida, 0, @anfitrion)",
                new Dictionary<string, object>() {
                    { "@idcuenta", idcuenta }, { "@idpartida", idpartida },
                    { "@anfitrion", asAnfitrion ? 1 : 0 }
                }
            );
        }

        public bool RemoveJugador(int idcuenta, int idpartida) {
            var player = GetJugador(idcuenta, idpartida);
            if (player.Id == 0) {
                return false;
            }
            return Database.ExecuteUpdate(
                "IF EXISTS (SELECT idjugador FROM jugador WHERE idjugador = @idjugador AND idpartida = @idpartida AND anfitrion = 1) " +
                "BEGIN " +
                    "DELETE FROM jugador WHERE idjugador = @idjugador AND idpartida = @idpartida; " +
                    "IF NOT EXISTS (SELECT TOP(1) idjugador FROM jugador WHERE idpartida = @idpartida) " +
                    "BEGIN " +
                        "DELETE FROM partida WHERE idpartida = @idpartida; " +
                    "END " +
                    "ELSE " +
                    "BEGIN " +
                        "UPDATE TOP(1) jugador SET anfitrion = 1 WHERE idpartida = @idpartida; " +
                    "END " +
                "END " +
                "ELSE IF EXISTS (SELECT idjugador FROM jugador WHERE idjugador = @idjugador AND idpartida = @idpartida) " +
                "BEGIN " +
                    "DELETE FROM jugador WHERE idjugador = @idjugador AND idpartida = @idpartida; " +
                    "IF NOT EXISTS (SELECT TOP(1) idjugador FROM jugador WHERE idpartida = @idpartida) " +
                    "BEGIN " +
                        "DELETE FROM partida WHERE idpartida = @idpartida; " +
                    "END " +
                "END",
                new Dictionary<string, object>() {
                    { "@idjugador", player.Id },
                    { "@idpartida", idpartida }
                }
            );
        }

        public bool CreatePartida(out int idpartida) {
            return Database.ExecuteUpdate(
                "INSERT INTO partida OUTPUT INSERTED.idpartida VALUES (0, @fecha)",
                new Dictionary<string, object>() {
                    { "@fecha", DateTime.Now }
                }, out idpartida
            );
        }
    }
}
