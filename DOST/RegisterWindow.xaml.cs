using System;
using System.Windows;

namespace DOST {
    /// <summary>
    /// Represents RegisterWindow.xaml interaction logic.
    /// </summary>
    public partial class RegisterWindow : Window {

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        public RegisterWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Handles RegisterButton click event. Establishes a connection with account service to try
        /// to register a new account with data given.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void RegisterButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(passwordPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(confirmPasswordPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(emailTextBox.Text) ||
                string.IsNullOrWhiteSpace(confirmEmailTextBox.Text)
            ) {
                MessageBox.Show(Properties.Resources.UncompletedFieldsErrorText);
                return;
            }
            if (!emailTextBox.Text.Contains("@")) {
                MessageBox.Show(Properties.Resources.InvalidDataErrorText);
                return;
            }
            if ((passwordPasswordBox.Password != confirmPasswordPasswordBox.Password) ||
                (emailTextBox.Text != confirmEmailTextBox.Text)
            ) {
                MessageBox.Show(Properties.Resources.UnmatchedRegisterFieldsErrorText);
                return;
            }
            Account newAccount = new Account(
                0, usernameTextBox.Text, passwordPasswordBox.Password,
                emailTextBox.Text, 0, DateTime.Now, false, null
            );
            if (newAccount.Register()) {
                MessageBox.Show(Properties.Resources.AccountRegisteredText);
                Close();
            } else {
                MessageBox.Show(Properties.Resources.AccountExistsRegisterErrorText);
            }
        }
    }
}
