using System.Collections.Generic;
using System.ServiceModel;

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
