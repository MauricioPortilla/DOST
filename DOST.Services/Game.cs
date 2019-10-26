using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
    [DataContract]
    public class Game {
        private int id;
        [DataMember]
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private int round;
        [DataMember]
        public int Round {
            get { return round; }
            set { round = value; }
        }
        private DateTime date;
        [DataMember]
        public DateTime Date {
            get { return date; }
            set { date = value; }
        }
        private List<Player> players;
        [DataMember]
        public List<Player> Players {
            get { return players; }
            set { players = value; }
        }
        private List<GameCategory> gameCategories;
        [DataMember]
        public List<GameCategory> GameCategories {
            get { return gameCategories; }
            set { gameCategories = value; }
        }
        [DataMember]
        public string ActiveGameGuid;
        [DataMember]
        public string LetterSelected;
        [DataMember]
        public long RoundStartingTime;

        public Game() {
        }

        public Game(int id, int round, DateTime date) {
            this.id = id;
            this.round = round;
            this.date = date;
            players = new List<Player>();
        }
    }
}
