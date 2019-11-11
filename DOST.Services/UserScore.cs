namespace DOST.Services {
    /// <summary>
    /// Represents a score from a player.
    /// </summary>
    public class UserScore {
        public int Ranking;
        public string Username;
        public int Score;

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
            Ranking = ranking;
            Username = username;
            Score = score;
        }
    }
}
