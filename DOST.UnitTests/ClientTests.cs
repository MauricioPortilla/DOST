using System;
using System.ServiceModel;
using DOST.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DOST.GameLobbyWindow;

namespace DOST.UnitTests {
    [TestClass]
    public class ClientTests {
        [TestMethod]
        public void LoginCuentaTest() {
            Account cuenta = new Account("Frey", "2506");
            Assert.AreEqual(true, cuenta.Login());
        }

        [TestMethod]
        public void RegisterCuentaTest() {
            Account cuenta = new Account(0, "TestUsuario", "1234", "test@test.com", 0, DateTime.Now, false, null);
            Assert.AreEqual(true, cuenta.Register());
        }

        [TestMethod]
        public void BroadcastChatMessage() {
            System.Windows.Controls.ListBox listBox = new System.Windows.Controls.ListBox();
            var callbackHandler = new ChatCallbackHandler(new Game(1, 0, DateTime.Now, new System.Collections.Generic.List<Player>()), listBox);
            InstanceContext chatInstance = new InstanceContext(callbackHandler);
            ChatServiceClient chatService = new ChatServiceClient(chatInstance);
            chatService.Open();
            if (chatService.State == CommunicationState.Opened) {
                chatService.EnterChat("", "Frey");
                chatService.BroadcastMessage("", "Frey", "Mensaje de prueba");
            }
            System.Threading.Thread.Sleep(1000);
            Assert.AreEqual("Mensaje de prueba", callbackHandler.LastMessageReceived);
        }
    }
}
