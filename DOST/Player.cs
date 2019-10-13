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
        private string activePlayerGuid;
        public string ActivePlayerGuid {
            get { return activePlayerGuid; }
            set { activePlayerGuid = value; }
        }
        private bool isReady = false;
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

        public bool LeaveGame(Game game) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.RemovePlayer(activePlayerGuid, game.ActiveGuidGame);
            });
        }

        public string GetRank() {
            var rank = Properties.Resources.NotRankedText;
            EngineNetwork.EstablishChannel<IAccountService>((service) => {
                var accountRank = service.GetRank(account.Id);
                rank = string.IsNullOrEmpty(accountRank) ? rank : accountRank;
                return true;
            });
            return rank;
        }

        public bool SetPlayerReady(bool isReady) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.SetPlayerReady(game.ActiveGuidGame, activePlayerGuid, isReady);
            });
        }
    }
}
