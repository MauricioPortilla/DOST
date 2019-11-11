using MaterialDesignThemes.Wpf;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DOST {
    /// <summary>
    /// Represents LoginWindow.xaml interaction logic.
    /// </summary>
    public partial class LoginWindow : Window {

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        public LoginWindow() {
            InitializeComponent();
            var language = Session.LANGUAGES.FirstOrDefault(languageItem => languageItem.Value == App.GetAppConfiguration()["DOST"]["Language"]).Key;
            languageSelectorComboBox.SelectedValue = language;
        }

        /// <summary>
        /// Handles LoginButton click event. Establishes a connection with account service to try login with
        /// entered credentials.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || string.IsNullOrWhiteSpace(passwordPasswordBox.Password)) {
                MessageBox.Show(Properties.Resources.EmptyFieldsErrorText);
                return;
            }
            Account account = new Account(usernameTextBox.Text, passwordPasswordBox.Password);
            DialogHost.Show(loadingStackPanel, "LoginWindow_WindowDialogHost", (openSender, openEventArgs) => {
                EngineNetwork.DoNetworkOperation(onExecute: () => {
                    if (!account.Login()) {
                        if (account.Id == 0) {
                            MessageBox.Show(Properties.Resources.LoginErrorText);
                        } else if (!account.IsVerified) {
                            MessageBox.Show(Properties.Resources.AccountNotConfirmedErrorText);
                        }
                        return false;
                    }
                    return true;
                }, onSuccess: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        passwordPasswordBox.Password = "";
                        Session.Account = account;
                        Session.MainMenuWindow = new MainMenuWindow();
                        Session.LoginWindow = this;
                        Session.MainMenuWindow.Show();
                        Hide();
                    });
                }, onFinish: () => {
                    Application.Current.Dispatcher.Invoke(delegate {
                        openEventArgs.Session.Close(true);
                    });
                }, false);
            }, null);
        }

        /// <summary>
        /// Handles RegisterButton click event. Shows a new register window.
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Button click event</param>
        private void RegisterButton_Click(object sender, RoutedEventArgs e) {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
        }

        /// <summary>
        /// Handles LanguageSelectorComboBox selection changed event. Changes language in configuration file.
        /// </summary>
        /// <param name="sender">ComboBox object</param>
        /// <param name="e">ComboBox selection changed event</param>
        private void LanguageSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            App.ChangeLanguage(Session.LANGUAGES[((ListBoxItem) e.AddedItems[0]).Content.ToString()]);
            MessageBox.Show(Properties.Resources.LanguageChangedText);
        }
    }
}
