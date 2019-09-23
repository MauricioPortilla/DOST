using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace DOST.Services {
    [ServiceContract]
    public interface IPartidaService {
        [OperationContract]
        List<Partida> GetPartidasList();

        [OperationContract]
        List<Jugador> GetJugadoresList(int idpartida);
        Jugador GetJugador(int idcuenta, int idpartida);

        [OperationContract]
        bool AddJugador(int idcuenta, int idpartida, bool asAnfitrion);

        [OperationContract]
        bool RemoveJugador(int idcuenta, int idpartida);

        [OperationContract]
        bool CreatePartida(out int idpartida);

        [OperationContract]
        List<CategoriaPartida> GetCategoriasList(int idpartida);
    }
}
