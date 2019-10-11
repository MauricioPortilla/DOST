using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window {
        public RegisterWindow() {
            InitializeComponent();
        }

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
