﻿using System.ServiceModel;

namespace DOST.Services {
    /// <summary>
    /// Interface for ingame service.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IInGameServiceCallback))]
    public interface IInGameService {
        [OperationContract(IsOneWay = true)]
        void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);

        [OperationContract(IsOneWay = true)]
        void EnterPlayer(string guidGame, string guidPlayer);

        [OperationContract(IsOneWay = true)]
        void LeavePlayer(string guidGame, string guidPlayer);

        [OperationContract(IsOneWay = true)]
        void StartRound(string guidGame, int playerSelectorIndex);

        [OperationContract(IsOneWay = true)]
        void StartGame(string guidGame);

        [OperationContract(IsOneWay = true)]
        void EndRound(string guidGame);

        [OperationContract(IsOneWay = true)]
        void PressDost(string guidGame, string guidPlayer);

        [OperationContract(IsOneWay = true)]
        void EndGame(string guidGame);

        [OperationContract(IsOneWay = true)]
        void ReduceTime(string guidGame, string guidPlayer);
    }

    /// <summary>
    /// Interface for ingame service callback.
    /// </summary>
    [ServiceContract]
    public interface IInGameServiceCallback {
        [OperationContract(IsOneWay = true)]
        void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);

        [OperationContract(IsOneWay = true)]
        void StartRound(string guidGame, int playerSelectorIndex);

        [OperationContract(IsOneWay = true)]
        void StartGame(string guidGame);

        [OperationContract(IsOneWay = true)]
        void EndRound(string guidGame);

        [OperationContract(IsOneWay = true)]
        void PressDost(string guidGame, string guidPlayer);

        [OperationContract(IsOneWay = true)]
        void EndGame(string guidGame);

        [OperationContract(IsOneWay = true)]
        void ReduceTime(string guidGame);
    }
}
