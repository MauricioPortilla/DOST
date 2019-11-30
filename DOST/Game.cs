using DOST.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DOST {

    /// <summary>
    /// Represents a game in the game.
    /// </summary>
    public class Game : INotifyPropertyChanged {
        private int id;
        public int Id {
            get { return id; }
        }
        private int round;
        public int Round {
            get { return round; }
            set { round = value; }
        }
        private DateTime date;
        public DateTime Date {
            get { return date; }
            set { date = value; }
        }
        private List<Player> players;
        public List<Player> Players {
            get { return players; }
            set {
                players = value;
                var host = players.Find(player => player.IsHost);
                if (host != null) {
                    if (App.Language == "en-US") {
                        Name = host.Account.Username + Properties.Resources.GameNameText;
                    } else {
                        Name = Properties.Resources.GameNameText + host.Account.Username;
                    }
                }
                if (numberOfPlayers != players.Count) {
                    NumberOfPlayers = players.Count.ToString();
                }
            }
        }
        private int numberOfPlayers;
        public string NumberOfPlayers {
            get { return numberOfPlayers + "/4"; }
            set {
                numberOfPlayers = int.Parse(value);
                NotifyPropertyChanged("NumberOfPlayers");
            }
        }
        public string Name { get; set; }
        private List<GameCategory> categories;
        public List<GameCategory> Categories {
            get { return categories; }
            set { categories = value; }
        }
        private string activeGuidGame;
        public string ActiveGuidGame {
            get { return activeGuidGame; }
            set { activeGuidGame = value; }
        }
        private string letterSelected;
        public string LetterSelected {
            get { return letterSelected; }
            set { letterSelected = value; }
        }
        private long roundStartingTime;
        public long RoundStartingTime {
            get { return roundStartingTime; }
            set { roundStartingTime = value; }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a Game instance with data given.
        /// </summary>
        /// <param name="id">Game identifier</param>
        /// <param name="round">Game round</param>
        /// <param name="date">Game creation date</param>
        /// <param name="players">Game Players</param>
        public Game(int id, int round, DateTime date, List<Player> players) {
            this.id = id;
            this.round = round;
            this.date = date;
            Players = players;
        }

        /// <summary>
        /// If any data from this Game changes, notifies it to an observer.
        /// </summary>
        /// <param name="obj">Object to be notified</param>
        private void NotifyPropertyChanged(string obj) {
            if (PropertyChanged != null) {
                this.PropertyChanged(this, new PropertyChangedEventArgs(obj));
            }
        }

        /// <summary>
        /// Establishes a connection with game service to add a new GameCategory to the game.
        /// </summary>
        /// <param name="category">GameCategory to be added</param>
        /// <returns>True if category was added successfully; False if not</returns>
        public bool AddCategory(GameCategory category) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.AddCategory(ActiveGuidGame, category.Name);
            });
        }

        /// <summary>
        /// Establishes a connection with game service to remove a GameCategory from the game.
        /// </summary>
        /// <param name="category">GameCategory to be removed</param>
        /// <returns>True if category was removed successfully; False if not</returns>
        public bool RemoveCategory(GameCategory category) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.RemoveCategory(ActiveGuidGame, category.Name);
            });
        }

        /// <summary>
        /// Establishes a connection with game service to start the game, increasing round value in 1.
        /// </summary>
        /// <returns>True if game was started successfully; False if not, or if round value was higher or equal to max rounds per game value</returns>
        public bool Start() {
            if (round >= Session.MAX_ROUNDS_PER_GAME) {
                return false;
            }
            round += 1;
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.StartGame(ActiveGuidGame);
            });
        }

        /// <summary>
        /// Establishes a connection with game service to set the letter to be played.
        /// </summary>
        /// <param name="selectRandomLetter">True if letter should be selected randomly</param>
        /// <param name="idaccount">Account identifier who made the choice</param>
        /// <param name="letter">Letter to be selected if random option was not selected</param>
        /// <returns>True if letter was set successfully; False if not</returns>
        public bool SetLetter(bool selectRandomLetter, int idaccount, string letter = null) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.SetGameLetter(ActiveGuidGame, idaccount, selectRandomLetter, letter);
            });
        }
    }
}
