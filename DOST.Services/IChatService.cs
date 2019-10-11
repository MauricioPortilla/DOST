using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceContract(CallbackContract = typeof(IChatServiceCallback))]
    public interface IChatService {
        [OperationContract(IsOneWay = true)]
        void BroadcastMessage(int idgame, string username, string message);

        [OperationContract(IsOneWay = true)]
        void EnterChat(int idgame, string username);

        [OperationContract(IsOneWay = true)]
        void LeaveChat(int idgame, string username);
    }


    [ServiceContract]
    public interface IChatServiceCallback {
        [OperationContract(IsOneWay = true)]
        void BroadcastMessage(int idgame, string username, string message);
    }
}
