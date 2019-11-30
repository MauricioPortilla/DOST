using DOST.Services;

namespace DOST {

    /// <summary>
    /// Provides methods associated to game service callback.
    /// </summary>
    public abstract class InGameCallbackHandler : IInGameServiceCallback {
        protected Game game;
        private bool messageReceived;
        public bool MessageReceived {
            get { return messageReceived; }
            set { messageReceived = value; }
        }

        public abstract void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);
        public abstract void StartRound(string guidGame, int playerSelectorIndex);
        public abstract void StartGame(string guidGame);
        public abstract void EndRound(string guidGame);
        public abstract void PressDost(string guidGame, string guidPlayer);
        public abstract void EndGame(string guidGame);
        public abstract void ReduceTime(string guidGame);
    }
}
