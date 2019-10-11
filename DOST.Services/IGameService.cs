using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceContract]
    public interface IGameService {
        [OperationContract]
        List<Game> GetGamesList();

        [OperationContract]
        List<Player> GetPlayersList(int idgame);
        Player GetPlayer(int idaccount, int idgame);

        [OperationContract]
        bool AddPlayer(int idaccount, int idgame, bool asHost);

        [OperationContract]
        bool RemovePlayer(int idaccount, int idgame);

        [OperationContract]
        bool CreateGame(out int idgame);

        [OperationContract]
        List<GameCategory> GetCategoriesList(int idgame);

        [OperationContract]
        bool AddCategory(int idgame, string name);

        [OperationContract]
        bool RemoveCategory(int idgame, int idcategory);

        [OperationContract]
        bool StartGame(int idgame);
    }
}
