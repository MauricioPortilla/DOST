using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceContract]
    public interface ICuentaService {
        [OperationContract]
        Cuenta TryLogin(string usuario, string password);

        [OperationContract]
        bool SignUp(Cuenta cuenta);

        [OperationContract]
        List<UserScore> GetBestScores();

        [OperationContract]
        string GetRank(int idcuenta);
    }
}
