using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;

namespace ProjektGameKatalog
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // alter connection code
      //private string connectionString = @"Server=LAPTOP-425O6GTS\SQLEXPRESS;Database=SpielKatalog;Trusted_Connection=True;";



      private string connectionString = @"Server=SAHAN\SQLEXPRESS;Database=SpielKatalog;Trusted_Connection=True;TrustServerCertificate=True;";
        
        
        public MainWindow()
        {
            InitializeComponent();
        }

        // Login-Button Click Event
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Bitte Benutzername und Passwort eingeben.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query, um die UserID abzurufen und die Anmeldeinformationen zu überprüfen
                    string query = "SELECT UserID FROM Users WHERE UserName = @username AND Passwords = @password"; SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    object result = command.ExecuteScalar(); // Holt die UserID oder null

                    if (result != null)
                    {
                        string userID = result.ToString(); // Die UserID direkt abrufen

                        MessageBox.Show("Login erfolgreich!");
                        // Öffne das Katalog-Fenster und übergib den Benutzernamen und die UserID
                        new Katalog(username, userID).Show(); // Übergabe der UserID
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Benutzername oder Passwort falsch.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Login: " + ex.Message);
            }
        }

        // Benutzer erstellen
        private void CreateUserButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Bitte Benutzername und Passwort eingeben.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Überprüfen, ob der Benutzername bereits existiert
                    string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE UserName = @username";
                    SqlCommand checkUserCmd = new SqlCommand(checkUserQuery, connection);
                    checkUserCmd.Parameters.AddWithValue("@username", username);
                    int userCount = (int)checkUserCmd.ExecuteScalar();

                    if (userCount > 0)
                    {
                        MessageBox.Show("Benutzername existiert bereits. Bitte wählen Sie einen anderen.");
                        return;
                    }

                    string userId = Guid.NewGuid().ToString().Substring(0, 5); // kleine eindeutige ID

                    string insertQuery = "INSERT INTO Users (UserID, UserName, Passwords) VALUES (@id, @username, @password)"; SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@id", userId);
                    insertCommand.Parameters.AddWithValue("@username", username);
                    insertCommand.Parameters.AddWithValue("@password", password);

                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Benutzer erfolgreich angelegt!");
                        // Melde den neu erstellten Benutzer automatisch an und übergib die Daten
                        new Katalog(username, userId).Show(); // Übergabe der UserID
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Benutzer konnte nicht angelegt werden.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Anlegen des Benutzers: " + ex.Message);
            }
        }
    }
}
