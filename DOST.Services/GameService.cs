using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DOST.DataAccess;

namespace DOST.Services {
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Single)]
    public class GameService : IGameService {
        public List<Game> GetGamesList() {
            List<Game> gamesList = new List<Game>();
            using (DostDatabase db = new DostDatabase()) {
                var gamesListDb = (from game in db.Game
                                   where game.round == 0 &&
                                   (from player in db.Player
                                    where player.idgame == game.idgame
                                    select player.idplayer).Count() < 4
                                   select game).ToList();
                gamesListDb.ForEach(
                    game => gamesList.Add(new Game(game.idgame, game.round, game.date))
                );
            }
            return gamesList;
        }

        public List<Player> GetPlayersList(int idgame) {
            List<Player> playersList = new List<Player>();
            using (DostDatabase db = new DostDatabase()) {
                var playersListDb = (from player in db.Player
                                     where player.idgame == idgame
                                     select player).ToList();
                playersListDb.ForEach(
                    player => playersList.Add(new Player(
                        player.idplayer,
                        new Account(
                            player.Account.idaccount, player.Account.username, 
                            player.Account.password, player.Account.email, 
                            player.Account.coins, player.Account.creationDate, 
                            player.Account.isVerified == 1, player.Account.validationCode
                        ),
                        null,
                        player.score,
                        player.isHost == 1
                    ))
                );
            }
            return playersList;
        }

        public Player GetPlayer(int idaccount, int idgame) {
            Player player = new Player();
            using (DostDatabase db = new DostDatabase()) {
                var playerDb = db.Player.ToList().Find(
                    playerList => playerList.idgame == idgame && playerList.idaccount == idaccount
                );
                if (playerDb != null) {
                    player.Id = playerDb.idplayer;
                    player.Account = new Account(
                        playerDb.Account.idaccount, playerDb.Account.username,
                        playerDb.Account.password, playerDb.Account.email,
                        playerDb.Account.coins, playerDb.Account.creationDate,
                        playerDb.Account.isVerified == 1, playerDb.Account.validationCode
                    );
                    player.Score = playerDb.score;
                    player.IsHost = playerDb.isHost == 1;
                    player.Game = null;
                }
            }
            return player;
        }

        public bool AddPlayer(int idaccount, int idgame, bool asHost) {
            using (DostDatabase db = new DostDatabase()) {
                db.Player.Add(new DataAccess.Player() {
                    idaccount = idaccount,
                    idgame = idgame,
                    score = 0,
                    isHost = asHost ? 1 : 0
                });
                return db.SaveChanges() != 0;
            }
        }

        public bool RemovePlayer(int idaccount, int idgame) {
            var player = GetPlayer(idaccount, idgame);
            if (player.Id == 0) {
                return false;
            }
            using (DostDatabase db = new DostDatabase()) {
                var playerHost = db.Player.ToList().Find(
                    playerDb => playerDb.idplayer == player.Id && 
                                playerDb.idgame == idgame && 
                                playerDb.isHost == 1
                );
                if (playerHost != null) {
                    db.Player.Remove(playerHost);
                    if ((db.Player.Where(playerDb => playerDb.idgame == idgame).Count() - 1) <= 0) {
                        db.GameCategory.ToList().ForEach(category => db.GameCategory.Remove(category));
                        db.Game.Remove(db.Game.ToList().Find(game => game.idgame == idgame));
                    } else {
                        db.Player.First(playerDatabase => playerDatabase.idgame == idgame).isHost = 1;
                    }
                } else {
                    var playerNotHost = db.Player.ToList().Find(
                        playerDb => playerDb.idplayer == player.Id &&
                                    playerDb.idgame == idgame
                    );
                    if (playerNotHost != null) {
                        db.Player.Remove(playerNotHost);
                        if (db.Player.Where(playerDb => playerDb.idgame == idgame).Count() == 0) {
                            db.GameCategory.ToList().ForEach(category => db.GameCategory.Remove(category));
                            db.Game.Remove(db.Game.ToList().Find(game => game.idgame == idgame));
                        }
                    }
                }
                return db.SaveChanges() != 0;
            }
        }

        public bool CreateGame(out int idgame) {
            using (DostDatabase db = new DostDatabase()) {
                var newGame = new DataAccess.Game() {
                    round = 0,
                    date = DateTime.Now
                };
                db.Game.Add(newGame);
                if (db.SaveChanges() != 0) {
                    idgame = newGame.idgame;
                    Engine.CategoriesList.ForEach((category) => {
                        db.GameCategory.Add(new DataAccess.GameCategory() {
                            idgame = newGame.idgame,
                            name = category
                        });
                    });
                    db.SaveChanges();
                    return true;
                }
            }
            idgame = 0;
            return false;
        }

        public List<GameCategory> GetCategoriesList(int idgame) {
            List<GameCategory> categoriesList = new List<GameCategory>();
            using (DostDatabase db = new DostDatabase()) {
                var categories = (from category in db.GameCategory
                                  where category.idgame == idgame
                                  select category).ToList();
                categories.ForEach(category => categoriesList.Add(
                    new GameCategory(category.idcategory, null, category.name)
                ));
            }
            return categoriesList;
        }

        public bool AddCategory(int idgame, string name) {
            using (DostDatabase db = new DostDatabase()) {
                var game = db.Game.Find(idgame);
                if (game == null) {
                    return false;
                }
                if (game.GameCategory.ToList().Find(
                    category => category.name == name && category.idgame == idgame
                ) != null) {
                    return false;
                }
                game.GameCategory.Add(new DataAccess.GameCategory {
                    idgame = game.idgame,
                    name = name
                });
                return db.SaveChanges() != 0;
            }
        }

        public bool RemoveCategory(int idgame, int idcategory) {
            using (DostDatabase db = new DostDatabase()) {
                var game = db.Game.Find(idgame);
                if (game == null) {
                    return false;
                }
                var categoria = game.GameCategory.ToList().Find(category => category.idcategory == idcategory);
                if (categoria == null) {
                    return false;
                }
                db.GameCategory.Remove(categoria);
                return db.SaveChanges() != 0;
            }
        }

        public bool StartGame(int idgame) {
            using (DostDatabase db = new DostDatabase()) {
                var game = db.Game.Find(idgame);
                if (game == null) {
                    return false;
                }
                game.round = 1;
                return db.SaveChanges() != 0;
            }
        }
    }
}
