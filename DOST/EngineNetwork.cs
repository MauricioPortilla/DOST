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
                return valueReturned;
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (EstablishChannel<" + typeof(IService).Name + ">) -> " + communicationException.Message + " | " + communicationException.StackTrace);
            } catch (Exception exception) {
                Console.WriteLine("Exception (EstablishChannel<" + typeof(IService).Name + ">) -> " + exception.Message + " | " + exception.StackTrace);
            }
            return false;
        }

        public static void DoNetworkAction(Func<bool> onExecute, Action onSuccess = null, Action onFinish = null, bool retryOnFail = false) {
            DoNetworkAction<Exception>(onExecute, onSuccess, onFinish, retryOnFail);
        }

        public static void DoNetworkAction<TException>(Func<bool> onExecute, Action onSuccess = null, Action onFinish = null, bool retryOnFail = false) where TException : Exception {
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
                                if (onExecute != null) {
                                    resultOnExecute = onExecute();
                                }
                            } catch (TException exception) {
                                Console.WriteLine("DoNetworkAction<" + typeof(TException).Name + "> Exception (OnExecute) -> " + exception.Message);
                                didThrowException = true;
                            }
                        }).ContinueWith((task) => {
                            if (!didThrowException) {
                                try {
                                    if (resultOnExecute) {
                                        onSuccess?.Invoke();
                                    }
                                } catch (Exception exception) {
                                    Console.WriteLine("DoNetworkAction<" + typeof(TException).Name + "> Exception (OnSuccess) -> " + exception.Message);
                                    didThrowException = true;
                                }
                            }
                            didFinish = true;
                        });
                    }
                    if (didStart && didFinish) {
                        if (didThrowException) {
                            if (retryOnFail) {
                                didStart = false;
                                didFinish = false;
                                continue;
                            }
                        }
                        try {
                            onFinish?.Invoke();
                        } catch (Exception exception) {
                            Console.WriteLine("DoNetworkAction<" + typeof(TException).Name + "> Exception (OnFinish) -> " + exception.Message);
                        }
                        break;
                    }
                }
            });
        }
    }
}
