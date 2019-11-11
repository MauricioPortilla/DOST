using DOST.Services;
using System.Collections.Generic;

namespace DOST {
    /// <summary>
    /// Represents a player in the game.
    /// </summary>
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

        /// <summary>
        /// Creates an instance with data given.
        /// </summary>
        /// <param name="id">Player identifier</param>
        /// <param name="account">Player account</param>
        /// <param name="game">Game associated with this player</param>
        /// <param name="score">Player score</param>
        /// <param name="isHost">Player role</param>
        public Player(int id, Account account, Game game, int score, bool isHost) {
            this.id = id;
            this.account = account;
            this.game = game;
            this.score = score;
            this.isHost = isHost;
        }

        /// <summary>
        /// Establishes a connection with game service to try to leave from the actual game.
        /// </summary>
        /// <param name="game">Game to leave</param>
        /// <returns>True if operation was successful; False if not</returns>
        public bool LeaveGame(Game game) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.RemovePlayer(activePlayerGuid, game.ActiveGuidGame);
            });
        }

        /// <summary>
        /// Returns actual player rank.
        /// </summary>
        /// <returns>Rank player</returns>
        public string GetRank() {
            return account.GetRank();
        }

        /// <summary>
        /// Establishes a connection with game service to try to set a new ready status.
        /// </summary>
        /// <param name="isReady">True if player is ready; False if not</param>
        /// <returns>True if operation was successful; False if not</returns>
        public bool SetPlayerReady(bool isReady) {
            return EngineNetwork.EstablishChannel<IGameService>((service) => {
                return service.SetPlayerReady(game.ActiveGuidGame, activePlayerGuid, isReady);
            });
        }

        /// <summary>
        /// Establishes a connection with game service to try to send the player answers from each game category.
        /// </summary>
        /// <param name="categoryPlayerAnswers">Category answers</param>
        /// <returns>True if operation was successful; False if not</returns>
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

        /// <summary>
        /// Establishes a connection with game service to get a game category word.
        /// </summary>
        /// <param name="category">GameCategory that needs the word</param>
        /// <returns>True if operation was successful; False if not</returns>
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
