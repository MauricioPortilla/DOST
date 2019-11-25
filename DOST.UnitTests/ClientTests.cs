using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DOST.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DOST.UnitTests {
    [TestClass]
    public class ClientTests {

        [TestMethod]
        public void RegisterAccountTest() {
            Account account = new Account(0, "TestUsuario", "1234", "test@test.com", 0, DateTime.Now, false, null);
            Assert.AreEqual(true, account.Register());
        }

        [TestMethod]
        public void RegisterExistentAccountTest() {
            Account account = new Account(0, "TestUsuario", "1234", "test@test.com", 0, DateTime.Now, false, null);
            Assert.AreEqual(false, account.Register());
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
            Assert.AreEqual(true, account.CreateGame(out string guidGame));
            Assert.AreNotEqual(string.Empty, guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;
            Assert.AreEqual(true, account.JoinGame(game, true, out string guidPlayer));
        }

        [TestMethod]
        public void GetRankTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            Assert.AreNotEqual(null, account.GetRank());
        }

        [TestMethod]
        public void AddGameCategoryTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;
            Assert.AreEqual(true, game.AddCategory(new GameCategory(0, game, "Category test name")));
        }

        [TestMethod]
        public void RemoveGameCategoryTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;
            game.AddCategory(new GameCategory(0, game, "Category test name"));
            Assert.AreEqual(true, game.RemoveCategory(new GameCategory(0, game, "Category test name")));
        }

        [TestMethod]
        public void StartGameTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;

            account.JoinGame(game, true, out string guidPlayer1);
            Player player1 = new Player(0, account, game, 0, true);
            player1.ActivePlayerGuid = guidPlayer1;
            player1.SetPlayerReady(true);

            Account account2 = new Account("Frey", "2506");
            account2.Login();
            account2.JoinGame(game, false, out string guidPlayer2);
            Player player2 = new Player(0, account2, game, 0, false);
            player2.ActivePlayerGuid = guidPlayer2;
            player2.SetPlayerReady(true);

            Assert.AreEqual(true, game.Start());
        }

        [TestMethod]
        public void SetGameLetterRandomTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;

            account.JoinGame(game, true, out string guidPlayer1);
            Player player1 = new Player(0, account, game, 0, true);
            player1.ActivePlayerGuid = guidPlayer1;
            player1.SetPlayerReady(true);

            Account account2 = new Account("Frey", "2506");
            account2.Login();
            account2.JoinGame(game, false, out string guidPlayer2);
            Player player2 = new Player(0, account2, game, 0, false);
            player2.ActivePlayerGuid = guidPlayer2;
            player2.SetPlayerReady(true);

            game.Start();
            Assert.AreEqual(true, game.SetLetter(true, account.Id));
        }

        [TestMethod]
        public void SetGameLetterSelectedTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;

            account.JoinGame(game, true, out string guidPlayer1);
            Player player1 = new Player(0, account, game, 0, true);
            player1.ActivePlayerGuid = guidPlayer1;
            player1.SetPlayerReady(true);

            Account account2 = new Account("Frey", "2506");
            account2.Login();
            account2.JoinGame(game, false, out string guidPlayer2);
            Player player2 = new Player(0, account2, game, 0, false);
            player2.ActivePlayerGuid = guidPlayer2;
            player2.SetPlayerReady(true);

            game.Start();
            Assert.AreEqual(true, game.SetLetter(false, account2.Id, "S"));
        }

        [TestMethod]
        public void LeaveGamePlayerTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;
            account.JoinGame(game, true, out string guidPlayer);
            Player player = new Player(0, account, game, 0, true);
            player.ActivePlayerGuid = guidPlayer;
            Assert.AreEqual(true, player.LeaveGame(game));
        }

        [TestMethod]
        public void SetPlayerReadyTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;
            account.JoinGame(game, true, out string guidPlayer1);
            Account account2 = new Account("Frey", "2506");
            account2.Login();
            account2.JoinGame(game, false, out string guidPlayer2);
            Player player = new Player(0, account2, game, 0, false);
            player.ActivePlayerGuid = guidPlayer2;
            Assert.AreEqual(true, player.SetPlayerReady(true));
        }

        [TestMethod]
        public void SendPlayerCategoryAnswersTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;

            account.JoinGame(game, true, out string guidPlayer1);
            Player player1 = new Player(0, account, game, 0, true);
            player1.ActivePlayerGuid = guidPlayer1;
            player1.SetPlayerReady(true);

            Account account2 = new Account("Frey", "2506");
            account2.Login();
            account2.JoinGame(game, false, out string guidPlayer2);
            Player player2 = new Player(0, account2, game, 0, false);
            player2.ActivePlayerGuid = guidPlayer2;
            player2.SetPlayerReady(true);

            game.Start();
            game.SetLetter(false, account.Id, "M");
            List<CategoryPlayerAnswer> answers = new List<CategoryPlayerAnswer>();
            answers.Add(new CategoryPlayerAnswer(0, player1, new GameCategory(0, game, "Nombre"), "Mauricio", 1));
            Assert.AreEqual(true, player1.SendCategoryAnswers(answers));
        }

        [TestMethod]
        public void GetCategoryWordTest() {
            Account account = new Account("TestUsuario", "1234");
            account.Login();
            account.CreateGame(out string guidGame);
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = guidGame;

            account.JoinGame(game, true, out string guidPlayer1);
            Player player1 = new Player(0, account, game, 0, true);
            player1.ActivePlayerGuid = guidPlayer1;
            player1.SetPlayerReady(true);

            Account account2 = new Account("Frey", "2506");
            account2.Login();
            account2.JoinGame(game, false, out string guidPlayer2);
            Player player2 = new Player(0, account2, game, 0, false);
            player2.ActivePlayerGuid = guidPlayer2;
            player2.SetPlayerReady(true);
            game.Start();
            game.SetLetter(false, account2.Id, "M");
            Assert.AreNotEqual(string.Empty, player2.GetCategoryWord(new GameCategory(0, game, "Nombre")));
        }

        [TestMethod]
        public void BroadcastChatMessageTest() {
            System.Windows.Controls.ListBox listBox = new System.Windows.Controls.ListBox();
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = "testguid";
            var callbackHandler = new ChatCallbackHandler(game, listBox);
            ChatServiceClient chatService = new ChatServiceClient(new InstanceContext(callbackHandler));
            chatService.EnterChat("testguid", "Frey");
            Thread.Sleep(1000);
            chatService.BroadcastMessage("testguid", "Frey", "Mensaje de prueba");
            Thread.Sleep(1000);
            Assert.AreEqual("Mensaje de prueba", callbackHandler.LastMessageReceived);
        }

        [TestMethod]
        public void BroadcastChatMessage_NotFromAnotherGame() {
            Game game = new Game(0, 0, DateTime.Now, new List<Player>());
            game.ActiveGuidGame = "testguid";
            var callbackHandlerFrey = new ChatCallbackHandler(game, new System.Windows.Controls.ListBox());
            ChatServiceClient chatServiceFrey = new ChatServiceClient(new InstanceContext(callbackHandlerFrey));
            chatServiceFrey.EnterChat("testguid", "Frey");
            Thread.Sleep(1000);

            Game game2 = new Game(0, 0, DateTime.Now, new List<Player>());
            game2.ActiveGuidGame = "testguid2";
            var callbackHandlerTester = new ChatCallbackHandler(game2, new System.Windows.Controls.ListBox());
            ChatServiceClient chatServiceTester = new ChatServiceClient(new InstanceContext(callbackHandlerTester));
            chatServiceTester.EnterChat("testguid2", "Tester");
            Thread.Sleep(1000);

            chatServiceTester.BroadcastMessage("testguid2", "Tester", "Mensaje de prueba");
            Thread.Sleep(1000);
            Assert.AreEqual("Mensaje de prueba", callbackHandlerTester.LastMessageReceived);
            Assert.AreEqual(string.Empty, callbackHandlerFrey.LastMessageReceived);
        }
    }
}
