using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceContract]
    public interface IAccountService {
        [OperationContract]
        Account TryLogin(string username, string password);

        [OperationContract]
        bool SignUp(Account account);

        [OperationContract]
        List<UserScore> GetBestScores();

        [OperationContract]
        string GetRank(int idaccount);
    }
}
