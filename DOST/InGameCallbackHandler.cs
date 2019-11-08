using DOST.Services;

namespace DOST {
    public abstract class InGameCallbackHandler : IInGameServiceCallback {
        protected Game game;

        public abstract void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);
        public abstract void StartRound(string guidGame, int playerSelectorIndex);
        public abstract void StartGame(string guidGame);
        public abstract void EndRound(string guidGame);
        public abstract void PressDost(string guidGame, string guidPlayer);
        public abstract void EndGame(string guidGame);
    }
}
