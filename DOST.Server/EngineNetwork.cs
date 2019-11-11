using DOST.Services;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DOST.Server {
    /// <summary>
    /// Manages server network.
    /// </summary>
    static class EngineNetwork {
        /// <summary>
        /// Service addresses.
        /// </summary>
        public static readonly Dictionary<string, Uri[]> URIS_SERVICES = new Dictionary<string, Uri[]>() {
            { "AccountService", new Uri[] {
                new Uri("net.tcp://localhost:25618/AccountService")
            } },
            { "GameService", new Uri[] {
                new Uri("net.tcp://localhost:25618/GameService")
            } },
            { "ChatService", new Uri[] {
                new Uri("net.tcp://localhost:25618/ChatService")
            } },
            { "InGameService", new Uri[] {
                new Uri("net.tcp://localhost:25618/InGameService")
            } }
        };

        /// <summary>
        /// Creates the hosts for all the services to establish connections.
        /// </summary>
        public static void CreateHosts() {
            try {
                ServiceHost loginHost = new ServiceHost(typeof(AccountService));
                loginHost.AddServiceEndpoint(typeof(IAccountService), new NetTcpBinding(SecurityMode.None), URIS_SERVICES["AccountService"][0]);
                loginHost.Opened += (sender, e) => {
                    Console.WriteLine("Account service opened.");
                };
                loginHost.Open();

                ServiceHost gameHost = new ServiceHost(typeof(GameService));
                gameHost.AddServiceEndpoint(typeof(IGameService), new NetTcpBinding(SecurityMode.None), URIS_SERVICES["GameService"][0]);
                gameHost.Opened += (sender, e) => {
                    Console.WriteLine("Game service opened.");
                };
                gameHost.Open();

                ServiceHost chatHost = new ServiceHost(typeof(ChatService));
                chatHost.AddServiceEndpoint(typeof(IChatService), new NetTcpBinding(SecurityMode.None), URIS_SERVICES["ChatService"][0]);
                chatHost.Opened += (sender, e) => {
                    Console.WriteLine("Chat service opened.");
                };
                chatHost.Open();

                ServiceHost inGameHost = new ServiceHost(typeof(InGameService));
                inGameHost.AddServiceEndpoint(typeof(IInGameService), new NetTcpBinding(SecurityMode.None), URIS_SERVICES["InGameService"][0]);
                inGameHost.Opened += (sender, e) => {
                    Console.WriteLine("In Game service opened.");
                };
                inGameHost.Open();
            } catch (ObjectDisposedException objectDisposedException) {
                Console.WriteLine("Object disposed exception -> " + objectDisposedException.Message);
            } catch (InvalidOperationException invalidOperationException) {
                Console.WriteLine("Invalid operation exception -> " + invalidOperationException.Message);
            } catch (CommunicationObjectFaultedException communicationOFException) {
                Console.WriteLine("Communication object faulted exception -> " + communicationOFException.Message);
            } catch (TimeoutException timeoutException) {
                Console.WriteLine("Timeout exception -> " + timeoutException.Message);
            } catch (Exception exception) {
                Console.WriteLine("Exception -> " + exception.Message);
            }
        }
    }
}
