using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceContract(CallbackContract = typeof(IInGameServiceCallback))]
    public interface IInGameService {
        [OperationContract(IsOneWay = true)]
        void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);

        [OperationContract(IsOneWay = true)]
        void EnterPlayer(string guidGame, string guidPlayer);

        [OperationContract(IsOneWay = true)]
        void LeavePlayer(string guidGame, string guidPlayer);

        [OperationContract(IsOneWay = true)]
        void StartRound(string guidGame);

        [OperationContract(IsOneWay = true)]
        void StartGame(string guidGame);
    }

    [ServiceContract]
    public interface IInGameServiceCallback {
        [OperationContract(IsOneWay = true)]
        void SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);

        [OperationContract(IsOneWay = true)]
        void StartRound(string guidGame);

        [OperationContract(IsOneWay = true)]
        void StartGame(string guidGame);
    }
}
