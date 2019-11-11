using System.ServiceModel;

namespace DOST.Services {
    /// <summary>
    /// Interface for chat service.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IChatServiceCallback))]
    public interface IChatService {
        [OperationContract(IsOneWay = true)]
        void BroadcastMessage(string guidGame, string username, string message);

        [OperationContract(IsOneWay = true)]
        void EnterChat(string guidGame, string username);

        [OperationContract(IsOneWay = true)]
        void LeaveChat(string guidGame, string username);
    }

    /// <summary>
    /// Interface for chat service callback.
    /// </summary>
    [ServiceContract]
    public interface IChatServiceCallback {
        [OperationContract(IsOneWay = true)]
        void BroadcastMessage(string guidGame, string username, string message);
    }
}
