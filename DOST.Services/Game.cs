using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DOST.Services {
    /// <summary>
    /// Represents a game in service.
    /// </summary>
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

        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        public Game() {
        }

        /// <summary>
        /// Creates and initializes a new instance with data given.
        /// </summary>
        /// <param name="id">Game identifier</param>
        /// <param name="round">Actual game round</param>
        /// <param name="date">GAme creation date</param>
        public Game(int id, int round, DateTime date) {
            this.id = id;
            this.round = round;
            this.date = date;
            players = new List<Player>();
        }
    }
}
