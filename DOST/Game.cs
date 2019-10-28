using DOST.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
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
                var host = players.Find(player => player.IsHost == true);
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
        public string ActiveGuidGame;
        public string LetterSelected;
        public long RoundStartingTime;
        public event PropertyChangedEventHandler PropertyChanged;

        public Game(int id, int round, DateTime date, List<Player> players) {
            this.id = id;
            this.round = round;
            this.date = date;
            Players = players;
        }

        private void NotifyPropertyChanged(string obj) {
            if (PropertyChanged != null) {
                this.PropertyChanged(this, new PropertyChangedEventArgs(obj));
            }
        }

        public bool AddCategory(GameCategory category) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.AddCategory(ActiveGuidGame, category.Name);
            });
        }

        public bool RemoveCategory(GameCategory category) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.RemoveCategory(ActiveGuidGame, category.Name);
            });
        }

        public bool Start() {
            round = 1;
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.StartGame(ActiveGuidGame);
            });
        }

        public bool SetLetter(bool selectRandomLetter, int idaccount, string letter = null) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.SetGameLetter(ActiveGuidGame, idaccount, selectRandomLetter, letter);
            });
        }
    }
}
