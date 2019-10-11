using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    static class EngineNetwork {
        public static readonly Dictionary<string, Uri[]> URIS_SERVICES = new Dictionary<string, Uri[]>() {
            { "CuentaService", new Uri[] {
                new Uri("net.tcp://" + App.ConnectionIP + ":" + App.ConnectionPort + "/AccountService")
            } },
            { "PartidaService", new Uri[] {
                new Uri("net.tcp://" + App.ConnectionIP + ":" + App.ConnectionPort + "/GameService")
            } },
            { "ChatService", new Uri[] {
                new Uri("net.tcp://" + App.ConnectionIP + ":" + App.ConnectionPort + "/ChatService")
            } }
        };
        private static readonly Dictionary<Type, string> CHANNEL_SERVICES = new Dictionary<Type, string>() {
            { typeof(IAccountService), "AccountService" },
            { typeof(IGameService), "GameService" },
            { typeof(IChatService), "ChatService" }
        };

        public static bool EstablishChannel<IService>(Func<IService, bool> onOpen) {
            var channel = new ChannelFactory<IService>(CHANNEL_SERVICES[typeof(IService)]);
            var serviceChannel = channel.CreateChannel();
            var valueReturned = onOpen.Invoke(serviceChannel);
            channel.Close();
            return valueReturned;
        }
    }
}
