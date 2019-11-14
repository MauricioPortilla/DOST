using System;
using System.ServiceModel;
using System.Threading.Tasks;
using DOST.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DOST.GameLobbyWindow;

namespace DOST.UnitTests {
    [TestClass]
    public class ClientTests {
        [TestMethod]
        public void RegisterAccountTest() {
            Account account = new Account(0, "TestUsuario", "1234", "test@test.com", 0, DateTime.Now, false, null);
            Assert.AreEqual(true, account.Register());
        }

        [TestMethod]
        public void LoginAccountTest() {
            Account account = new Account("TestUsuario", "1234");
            Assert.AreEqual(true, account.Login());
        }

        [TestMethod]
        public void ReloadAccountTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            Assert.AreEqual(true, account.Reload());
        }

        [TestMethod]
        public void CreateGameAndJoinTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            var guidGame = string.Empty;
            Assert.AreEqual(true, account.CreateGame(out guidGame));
            Assert.AreNotEqual(string.Empty, guidGame);
            Game game = new Game(0, 0, DateTime.Now, new System.Collections.Generic.List<Player>());
            game.ActiveGuidGame = guidGame;
            Assert.AreEqual(true, account.JoinGame(game, true));
        }

        [TestMethod]
        public void GetRankTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            Assert.AreNotEqual(null, account.GetRank());
        }

        [TestMethod]
        public void BroadcastChatMessage() {
            System.Windows.Controls.ListBox listBox = new System.Windows.Controls.ListBox();
            var callbackHandler = new ChatCallbackHandler(
                new Game(0, 0, DateTime.Now, new System.Collections.Generic.List<Player>()), listBox
            );
            InstanceContext chatInstance = new InstanceContext(callbackHandler);
            ChatServiceClient chatService = new ChatServiceClient(chatInstance);
            Task.Run(() => {
                chatService.Open();
                if (chatService.State == CommunicationState.Opened) {
                    chatService.EnterChat("", "Frey");
                    chatService.BroadcastMessage("", "Frey", "Mensaje de prueba");
                }
                System.Threading.Thread.Sleep(2000);
            }).Wait();
            Assert.AreEqual("Mensaje de prueba", callbackHandler.LastMessageReceived);
        }
    }
}
