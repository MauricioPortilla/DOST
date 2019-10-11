using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class Player {
        private int id;
        private Account account;
        public Account Account {
            get { return account; }
        }
        private Game game;
        private int score;
        private bool isHost;
        public bool IsHost {
            get { return isHost; }
        }

        public Player(int id, Account account, Game game, int score, bool isHost) {
            this.id = id;
            this.account = account;
            this.game = game;
            this.score = score;
            this.isHost = isHost;
        }

        public string GetRank() {
            var rank = "No clasificado";
            EngineNetwork.EstablishChannel<IAccountService>((service) => {
                rank = service.GetRank(account.Id);
                return true;
            });
            return rank;
        }
    }
}
