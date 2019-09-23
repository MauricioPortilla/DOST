using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Server {
    static class EngineNetwork {
        public static readonly Dictionary<string, Uri[]> URIS_SERVICES = new Dictionary<string, Uri[]>() {
            { "CuentaService", new Uri[] {
                new Uri("net.tcp://localhost:25618/CuentaService")
            } },
            { "PartidaService", new Uri[] {
                new Uri("net.tcp://localhost:25618/PartidaService")
            } },
            { "ChatService", new Uri[] {
                new Uri("net.tcp://localhost:25618/ChatService")
            } }
        };

        public static void CreateHosts() {
            try {
                ServiceHost loginHost = new ServiceHost(typeof(CuentaService), URIS_SERVICES["CuentaService"]);
                loginHost.AddServiceEndpoint(typeof(ICuentaService), new NetTcpBinding(SecurityMode.None), "");
                loginHost.Opened += (sender, e) => {
                    Console.WriteLine("Cuenta service opened.");
                };
                loginHost.Open();

                ServiceHost partidaHost = new ServiceHost(typeof(PartidaService), URIS_SERVICES["PartidaService"]);
                var binding = new NetTcpBinding(SecurityMode.None);
                binding.MaxBufferPoolSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxReceivedMessageSize = 2147483647;
                binding.OpenTimeout = TimeSpan.FromMinutes(59);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(59);
                binding.SendTimeout = TimeSpan.FromMinutes(59);
                partidaHost.AddServiceEndpoint(typeof(IPartidaService), binding, "");
                partidaHost.Opened += (sender, e) => {
                    Console.WriteLine("Partida service opened.");
                };
                partidaHost.Open();

                ServiceHost chatHost = new ServiceHost(typeof(ChatService), URIS_SERVICES["ChatService"]);
                chatHost.AddServiceEndpoint(typeof(IChatService), new NetTcpBinding(SecurityMode.None), "");
                chatHost.Opened += (sender, e) => {
                    Console.WriteLine("Chat service opened.");
                };
                chatHost.Open();
            } catch (Exception e) {
                Console.WriteLine("Exception -> " + e.Message);
            }
        }
    }
}
