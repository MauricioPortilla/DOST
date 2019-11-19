using DOST.Services;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace DOST {

    /// <summary>
    /// Manages connections between client and server services.
    /// </summary>
    public static class EngineNetwork {

        /// <summary>
        /// Server services
        /// </summary>
        private static readonly Dictionary<Type, string> CHANNEL_SERVICES = new Dictionary<Type, string>() {
            { typeof(IAccountService), "AccountService" },
            { typeof(IGameService), "GameService" },
            { typeof(IChatService), "ChatService" }
        };

        /// <summary>
        /// Establishes a connection with a service and executes an operation when connection is opened.
        /// </summary>
        /// <typeparam name="IService">Service interface</typeparam>
        /// <param name="onOpen">Operation to execute on connection opening</param>
        /// <returns>True if operation and connection closure was successful; False if not</returns>
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

        /// <summary>
        /// Executes a network operation asynchronously.
        /// </summary>
        /// <param name="onExecute">Operation to execute</param>
        /// <param name="onSuccess">Operation to execute if onExecute operation was successful</param>
        /// <param name="onFinish">Operation to execute if onExecute and onSuccess operation was successful</param>
        /// <param name="retryOnFail">True if should retry onExecute operation if an exception is throwed in onExecute or onSuccess operation</param>
        public static void DoNetworkOperation(Func<bool> onExecute, Action onSuccess = null, Action onFinish = null, bool retryOnFail = false) {
            DoNetworkOperation<Exception>(onExecute, onSuccess, onFinish, retryOnFail);
        }

        /// <summary>
        /// Executes a network operation asynchronously.
        /// </summary>
        /// <typeparam name="TException">Exception to handle if necessary in onExecute operation</typeparam>
        /// <param name="onExecute">Operation to execute</param>
        /// <param name="onSuccess">Operation to execute if onExecute operation was successful</param>
        /// <param name="onFinish">Operation to execute if onExecute and onSuccess operation was successful</param>
        /// <param name="retryOnFail">True if should retry onExecute operation if an exception is throwed in onExecute or onSuccess operation</param>
        public static void DoNetworkOperation<TException>(Func<bool> onExecute, Action onSuccess = null, Action onFinish = null, bool retryOnFail = false) where TException : Exception {
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
