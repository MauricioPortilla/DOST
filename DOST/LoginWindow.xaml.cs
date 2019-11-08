using MaterialDesignThemes.Wpf;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DOST {
    /// <summary>
    /// Lógica de interacción para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {
        public LoginWindow() {
            InitializeComponent();
            var language = Session.LANGUAGES.FirstOrDefault(languageItem => languageItem.Value == App.GetAppConfiguration()["DOST"]["Language"]).Key;
            languageSelectorComboBox.SelectedValue = language;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || string.IsNullOrWhiteSpace(passwordPasswordBox.Password)) {
                MessageBox.Show(Properties.Resources.EmptyFieldsErrorText);
                return;
            }
            Account account = new Account(usernameTextBox.Text, passwordPasswordBox.Password);
            DialogHost.Show(loadingStackPanel, "LoginWindow_WindowDialogHost", (openSender, openEventArgs) => {
                EngineNetwork.DoNetworkAction(onExecute: () => {
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

        private void RegisterButton_Click(object sender, RoutedEventArgs e) {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
        }

        private void LanguageSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            App.ChangeLanguage(Session.LANGUAGES[((ListBoxItem) e.AddedItems[0]).Content.ToString()]);
            MessageBox.Show(Properties.Resources.LanguageChangedText);
        }
    }
}
