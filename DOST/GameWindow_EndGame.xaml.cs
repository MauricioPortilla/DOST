﻿using DOST.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para GameWindow_EndGame.xaml
    /// </summary>
    public partial class GameWindow_EndGame : Window {
        public bool IsClosed { get; private set; } = false;
        private Game game;
        private Player player;
        private ChatServiceClient chatService;
        private List<TextBlock> lobbyPlayersUsernameTextBlocks;
        private List<TextBlock> lobbyPlayersScoreTextBlocks;
        private List<TextBlock> lobbyPlayersPlaceTextBlocks;
        private List<TextBlock> lobbyPlayersPlaceTitleTextBlocks;
        private List<Player> playerPlaces = new List<Player>();

        public GameWindow_EndGame(Game game) {
            InitializeComponent();
            try {
                this.game = Session.AllGamesAvailable.First(gameList => gameList.ActiveGuidGame == game.ActiveGuidGame);
                player = game.Players.Find(playerInGame => playerInGame.Account.Id == Session.Account.Id);
                Session.JoinGameChat(game, player, chatListBox, ref chatService);
            } catch (CommunicationException communicationException) {
                Console.WriteLine("CommunicationException (GameLobbyWindow) -> " + communicationException.Message);
                MessageBox.Show(Properties.Resources.CouldntJoinToGameErrorText);
                Close();
                return;
            }
            lobbyPlayersUsernameTextBlocks = new List<TextBlock>() {
                playerOneUsernameTextBlock, playerTwoUsernameTextBlock,
                playerThreeUsernameTextBlock, playerFourUsernameTextBlock
            };
            lobbyPlayersScoreTextBlocks = new List<TextBlock>() {
                playerOneScoreTextBlock, playerTwoScoreTextBlock,
                playerThreeScoreTextBlock, playerFourScoreTextBlock
            };
            lobbyPlayersPlaceTextBlocks = new List<TextBlock>() {
                playerOnePlaceTextBlock, playerTwoPlaceTextBlock,
                playerThreePlaceTextBlock, playerFourPlaceTextBlock
            };
            lobbyPlayersPlaceTitleTextBlocks = new List<TextBlock>() {
                playerOnePlaceTitleTextBlock, playerTwoPlaceTitleTextBlock,
                playerThreePlaceTitleTextBlock, playerFourPlaceTitleTextBlock
            };
            CalculatePlaces();
            LoadPlayers();
            Session.Account.Coins += Session.MAX_COINS_PER_GAME_WIN / (playerPlaces.IndexOf(playerPlaces.Find(playerPlace => playerPlace.ActivePlayerGuid == player.ActivePlayerGuid)) + 1);
        }

        private void CalculatePlaces() {
            playerPlaces = game.Players.ToList();
            playerPlaces.OrderBy(playerPlace => playerPlace.Score);
        }

        private void LoadPlayers() {
            var players = game.Players;
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++) {
                lobbyPlayersUsernameTextBlocks[playerIndex].Text = players[playerIndex].Account.Username;
                lobbyPlayersUsernameTextBlocks[playerIndex].Visibility = Visibility.Visible;
                lobbyPlayersPlaceTextBlocks[playerIndex].Text = "#" + (playerPlaces.IndexOf(playerPlaces.Find(playerPlace => playerPlace.ActivePlayerGuid == players[playerIndex].ActivePlayerGuid)) + 1);
                lobbyPlayersPlaceTextBlocks[playerIndex].Visibility = Visibility.Visible;
                lobbyPlayersScoreTextBlocks[playerIndex].Text += ": " + players[playerIndex].Score;
                lobbyPlayersScoreTextBlocks[playerIndex].Visibility = Visibility.Visible;
                lobbyPlayersPlaceTitleTextBlocks[playerIndex].Visibility = Visibility.Visible;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) {
            if (player.LeaveGame(game)) {
                try {
                    chatService.LeaveChat(game.ActiveGuidGame, player.Account.Username);
                    Close();
                } catch (CommunicationException communicationException) {
                    Console.WriteLine("CommunicationException (ExitButton_Click) -> " + communicationException.Message);
                    MessageBox.Show(Properties.Resources.AnErrorHasOcurredErrorText);
                }
            }
        }

        private void ChatMessageTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (string.IsNullOrWhiteSpace(chatMessageTextBox.Text)) {
                    return;
                }
                EngineNetwork.DoNetworkOperation<CommunicationException>(onExecute: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        chatService.BroadcastMessage(game.ActiveGuidGame, player.Account.Username, chatMessageTextBox.Text);
                        chatMessageTextBox.Clear();
                    });
                    return true;
                });
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            IsClosed = true;
            Session.IsPlayerInGame = false;
            Session.MainMenuWindow.Show();
        }

        private void WindowHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
