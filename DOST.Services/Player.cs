using System.Runtime.Serialization;

namespace DOST.Services {
    /// <summary>
    /// Represents a player in services.
    /// </summary>
    [DataContract]
    public class Player {
        private int id;
        [DataMember]
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Account account;
        [DataMember]
        public Account Account {
            get { return account; }
            set { account = value; }
        }
        private Game game;
        [DataMember]
        public Game Game {
            get { return game; }
            set { game = value; }
        }
        private int score;
        [DataMember]
        public int Score {
            get { return score; }
            set { score = value; }
        }
        private bool isHost;
        [DataMember]
        public bool IsHost {
            get { return isHost; }
            set { isHost = value; }
        }
        private string activePlayerGuid;
        [DataMember]
        public string ActivePlayerGuid {
            get { return activePlayerGuid; }
            set { activePlayerGuid = value; }
        }
        private bool isReady = false;
        [DataMember]
        public bool IsReady {
            get { return isReady; }
            set { isReady = value; }
        }

        /// <summary>
        /// Creates and initiailizes a new instance with data given.
        /// </summary>
        /// <param name="id">Player identifier</param>
        /// <param name="account">Account identifier</param>
        /// <param name="game">Game where players belongs to</param>
        /// <param name="score">Player score</param>
        /// <param name="isHost">True if player is the game host; False if not</param>
        public Player(int id, Account account, Game game, int score, bool isHost) {
            this.id = id;
            this.account = account;
            this.game = game;
            this.score = score;
            this.isHost = isHost;
        }

        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        public Player() {
        }
    }
}
