using DOST.Services;
using System.Collections.Generic;

namespace DOST {
    public class Player {
        private int id;
        private Account account;
        public Account Account {
            get { return account; }
        }
        private Game game;
        private int score;
        public int Score {
            get { return score; }
            set { score = value; }
        }
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
            return account.GetRank();
        }

        public bool SetPlayerReady(bool isReady) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.SetPlayerReady(game.ActiveGuidGame, activePlayerGuid, isReady);
            });
        }

        public bool SendCategoryAnswers(List<CategoryPlayerAnswer> categoryPlayerAnswers) {
            List<Services.CategoryPlayerAnswer> categoryPlayerAnswersService = new List<Services.CategoryPlayerAnswer>();
            foreach (var categoryPlayerAnswer in categoryPlayerAnswers) {
                categoryPlayerAnswersService.Add(new Services.CategoryPlayerAnswer {
                    Answer = categoryPlayerAnswer.Answer,
                    Round = categoryPlayerAnswer.Round,
                    GameCategory = new Services.GameCategory(0, null, categoryPlayerAnswer.GameCategory.Name)
                });
            }
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.SendCategoryAnswers(game.ActiveGuidGame, activePlayerGuid, categoryPlayerAnswersService);
            });
        }

        public string GetCategoryWord(GameCategory category) {
            var word = string.Empty;
            EngineNetwork.EstablishChannel<IGameService>((service) => {
                word = service.GetCategoryWord(game.ActiveGuidGame, activePlayerGuid, category.Name);
                return true;
            });
            return word;
        }
    }
}
