using DOST.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
            var valueReturned = false;
            try {
                var serviceChannel = channel.CreateChannel();
                valueReturned = onOpen.Invoke(serviceChannel);
                channel.Close();
            } catch (CommunicationException communicationException) {
                Console.WriteLine("Communication exception -> " + communicationException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
            return valueReturned;
        }

        public static void DoNetworkAction(Func<bool> onExecute, Action onSuccess, Action onFinish, bool retryOnFail) {
            Task.Run(() => {
                bool resultOnExecute = false;
                bool didStart = false;
                bool didFinish = false;
                bool didThrowException = false;
                while (true) {
                    if (!didStart) {
                        didStart = true;
                        var taskToExecute = Task.Run(() => {
                            try {
                                resultOnExecute = onExecute();
                            } catch (Exception exception) {
                                Console.WriteLine("DoNetworkAction Exception (OnExecute) -> " + exception.Message);
                                didThrowException = true;
                                throw exception;
                            }
                        }).ContinueWith((task) => {
                            if (task.Exception == null) {
                                try {
                                    if (resultOnExecute) {
                                        onSuccess();
                                    }
                                } catch (Exception exception) {
                                    Console.WriteLine("DoNetworkAction Exception (OnSuccess) -> " + exception.Message);
                                    didThrowException = true;
                                }
                            }
                            didFinish = true;
                        });
                    }
                    if (didStart && didFinish) {
                        if (didThrowException) {
                            if (retryOnFail) {
                                continue;
                            }
                        }
                        try {
                            onFinish();
                        } catch (Exception exception) {
                            Console.WriteLine("DoNetworkAction Exception (OnFinish) -> " + exception.Message);
                        }
                        break;
                    }
                }
            });
        }
    }
}
