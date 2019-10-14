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
        bool AddPlayer(int idaccount, string guidGame, bool asHost);

        [OperationContract]
        bool RemovePlayer(string guidPlayer, string guidGame);

        [OperationContract]
        bool CreateGame(out string guidGame);

        [OperationContract]
        List<GameCategory> GetCategoriesList(int idgame);

        [OperationContract]
        bool AddCategory(string guidGame, string name);

        [OperationContract]
        bool RemoveCategory(string guidGame, string name);

        [OperationContract]
        bool SetPlayerReady(string guidGame, string guidPlayer, bool isPlayerReady);

        [OperationContract]
        bool StartGame(string guidGame);
    }
}
