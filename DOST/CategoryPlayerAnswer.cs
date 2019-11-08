namespace DOST {

    /// <summary>
    /// Represents an answer written by a Player for a GameCategory
    /// </summary>
    public class CategoryPlayerAnswer {
        private int id;
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Player player;
        public Player Player {
            get { return player; }
            set { player = value; }
        }
        private GameCategory gameCategory;
        public GameCategory GameCategory {
            get { return gameCategory; }
            set { gameCategory = value; }
        }
        private string answer;
        public string Answer {
            get { return answer; }
            set { answer = value; }
        }
        private int round;
        public int Round {
            get { return round; }
            set { round = value; }
        }
        private bool hasCorrectAnswer;
        public bool HasCorrectAnswer {
            get { return hasCorrectAnswer; }
            set { hasCorrectAnswer = value; }
        }

        /// <summary>
        /// Creates a CategoryPlayerAnswer instance based on data given.
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <param name="player">Player who wrote the answer</param>
        /// <param name="gameCategory">GameCategory where the answer belongs to</param>
        /// <param name="answer">Answer written by the Player</param>
        /// <param name="round">Round where the answer was written</param>
        public CategoryPlayerAnswer(int id, Player player, GameCategory gameCategory, string answer, int round) {
            this.id = id;
            this.player = player;
            this.gameCategory = gameCategory;
            this.answer = answer;
            this.round = round;
        }
    }
}
