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
                MessageBox.Show("Faltan campos por completar.");
                return;
            }
            if (!emailTextBox.Text.Contains("@")) {
                MessageBox.Show("Debes introducir datos válidos.");
                return;
            }
            if ((passwordPasswordBox.Password != confirmPasswordPasswordBox.Password) ||
                (emailTextBox.Text != confirmEmailTextBox.Text)
            ) {
                MessageBox.Show("El correo o la contraseña no coincide con su campo de confirmación.");
                return;
            }
            Cuenta nuevaCuenta = new Cuenta(
                0, usernameTextBox.Text, passwordPasswordBox.Password,
                emailTextBox.Text, 0, DateTime.Now, false, null
            );
            if (nuevaCuenta.Register()) {
                MessageBox.Show("Cuenta registrada. Por favor, revisa tu correo para activar tu cuenta.");
                Close();
            } else {
                MessageBox.Show("Ya existe una cuenta registrada con ese nombre de usuario o correo.");
            }
        }
    }
}
