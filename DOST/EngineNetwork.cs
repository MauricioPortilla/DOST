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
                new Uri("net.tcp://" + App.ConnectionIP + ":" + App.ConnectionPort + "/CuentaService")
            } },
            { "PartidaService", new Uri[] {
                new Uri("net.tcp://" + App.ConnectionIP + ":" + App.ConnectionPort + "/PartidaService")
            } },
            { "ChatService", new Uri[] {
                new Uri("net.tcp://" + App.ConnectionIP + ":" + App.ConnectionPort + "/ChatService")
            } }
        };
        private static readonly Dictionary<Type, string> CHANNEL_SERVICES = new Dictionary<Type, string>() {
            { typeof(ICuentaService), "CuentaService" },
            { typeof(IPartidaService), "PartidaService" },
            { typeof(IChatService), "ChatService" }
        };

        public static bool EstablishChannel<IService>(Func<IService, bool> onOpen) {
            var binding = new NetTcpBinding(SecurityMode.None);
            binding.MaxBufferSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.OpenTimeout = TimeSpan.FromMinutes(59);
            binding.SendTimeout = TimeSpan.FromMinutes(59);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(59);
            var channel = new ChannelFactory<IService>(
                binding,
                new EndpointAddress(URIS_SERVICES[CHANNEL_SERVICES[typeof(IService)]][0])
            );
            var serviceChannel = channel.CreateChannel();
            var valueReturned = onOpen.Invoke(serviceChannel);
            channel.Close();
            return valueReturned;
        }

        /*public static bool EstablishDuplexChannel<IService>(Func<IService, bool> onOpen) {
            var binding = new NetTcpBinding(SecurityMode.None);
            binding.MaxBufferSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.OpenTimeout = TimeSpan.FromMinutes(20);
            binding.SendTimeout = TimeSpan.FromMinutes(20);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(20);
            var duplexChannel = new DuplexChannelFactory<IService>(
                binding,
                new EndpointAddress(URIS_SERVICES[CHANNEL_SERVICES[typeof(IService)]][0])
            );
        }*/
    }
}
