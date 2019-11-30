namespace DOST.Services {
    /// <summary>
    /// Represents a score from a player.
    /// </summary>
    public class UserScore {
        private int ranking;
        public int Ranking {
            get { return ranking; }
            set { ranking = value; }
        }
        private string username;
        public string Username {
            get { return username; }
            set { username = value; }
        }
        private int score;
        public int Score {
            get { return score; }
            set { score = value; }
        }

        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        public UserScore() {
        }

        /// <summary>
        /// Creates and initializes a new instance with data given.
        /// </summary>
        /// <param name="ranking">Player place</param>
        /// <param name="username">Player username</param>
        /// <param name="score">Player score</param>
        public UserScore(int ranking, string username, int score) {
            this.ranking = ranking;
            this.username = username;
            this.score = score;
        }
    }
}
