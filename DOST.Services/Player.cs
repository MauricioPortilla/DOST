using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
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

        public Player(int id, Account account, Game game, int score, bool isHost) {
            this.id = id;
            this.account = account;
            this.game = game;
            this.score = score;
            this.isHost = isHost;
        }

        public Player() {
        }
    }
}
