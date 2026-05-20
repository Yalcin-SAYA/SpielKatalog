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
using System.Data.SqlClient; // Hinzugefügt für SQL-Verbindung
using SqlConnection = Microsoft.Data.SqlClient.SqlConnection;
using SqlCommand = Microsoft.Data.SqlClient.SqlCommand;


namespace ProjektGameKatalog // Namespace geändert
{
    /// <summary>
    /// Interaction logic for RPG.xaml
    /// </summary>
    public partial class RPG : Window 
    {
        int[,] map = new int[20, 20];
        int playerX = 0;
        int playerY = 0;
        int playerHp = 100;
        int playerGold = 0;
        Random rnd = new Random();

        private string _loggedInUsername; // Speichert den Benutzernamen
        private string _loggedInUserID;   // Speichert die UserID

        //Benutzernamen und die UserID zu empfangen
        public RPG(string username, string userID)
        {
            InitializeComponent();
            _loggedInUsername = username;
            _loggedInUserID = userID; // UserID speichern
            InitializeMap();
            DrawMap();
            UpdateStatus();
        }

        ////für Tests ohne Login
        //public RPG() : this("Gast", null) // mit Standardwerten aufrufen
        //{
        //    // Initialisierung, falls nötig
        //}

        private void InitializeMap()
        {
            // Karte füllen mit 0 (leer)
            for (int y = 0; y < 20; y++)
                for (int x = 0; x < 20; x++)
                    map[y, x] = 0;

            // Wände zufällig (z.B. 110 Stück)
            int wallCount = 110;
            for (int i = 0; i < wallCount; i++)
            {
                int wx = rnd.Next(20);
                int wy = rnd.Next(20);
                if ((wx == 0 && wy == 0) || (wx == 19 && wy == 19)) continue;
                if (map[wy, wx] == 0)
                    map[wy, wx] = 1;
            }

            // Gegner (z.B. 50)
            int enemyCount = 50;
            for (int i = 0; i < enemyCount; i++)
            {
                int gx = rnd.Next(20);
                int gy = rnd.Next(20);
                if (map[gy, gx] == 0)
                    map[gy, gx] = 2;
            }

            // Fragezeichen-Events (z.B. 40)
            int eventCount = 40;
            for (int i = 0; i < eventCount; i++)
            {
                int ex = rnd.Next(20);
                int ey = rnd.Next(20);
                if (map[ey, ex] == 0)
                    map[ey, ex] = 3;
            }

            // Boss (1 Stück)
            while (true)
            {
                int bx = rnd.Next(20);
                int by = rnd.Next(20);
                if (map[by, bx] == 0)
                {
                    map[by, bx] = 6;
                    break;
                }
            }

            // Heilfelder (2 Stück)
            for (int i = 0; i < 2; i++)
            {
                while (true)
                {
                    int hx = rnd.Next(20);
                    int hy = rnd.Next(20);
                    if (map[hy, hx] == 0 && !(hx == 0 && hy == 0) && !(hx == 19 && hy == 19))
                    {
                        map[hy, hx] = 7;
                        break;
                    }
                }
            }

            // Ziel setzen
            map[19, 19] = 4;

            // Spielerstart
            playerX = 0;
            playerY = 0;
            map[playerY, playerX] = 5;
        }

        private void DrawMap()
        {
            MapGrid.Children.Clear();
            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    Button btn = new Button
                    {
                        IsEnabled = false,
                        Margin = new Thickness(1),
                        Background = Brushes.White,
                    };

                    Image img = new Image
                    {
                        Width = 50,
                        Height = 50,
                        Stretch = Stretch.Uniform
                    };

                    switch (map[y, x])
                    {
                        case 0: // leer
                            btn.Background = Brushes.White;
                            btn.Content = null;
                            break;
                        case 1: // Wand
                            img.Source = new BitmapImage(new Uri("C:\\Users\\yalci\\Downloads\\WPF Projekt Sahan\\WPF Projekt Sahan\\ProjektGameKatalog Sahan\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Wand.PNG")); 
                            btn.Content = img;
                            break;
                        case 2: // Gegner
                            img.Source = new BitmapImage(new Uri("C:\\Users\\yalci\\Downloads\\WPF Projekt Sahan\\WPF Projekt Sahan\\ProjektGameKatalog Sahan\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\RPG Boss.PNG")); 
                            btn.Content = img;
                            break;
                        case 3: // Fragezeichen-Event
                            img.Source = new BitmapImage(new Uri("C:\\Users\\yalci\\Downloads\\WPF Projekt Sahan\\WPF Projekt Sahan\\ProjektGameKatalog Sahan\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Zufall.PNG")); 
                            btn.Content = img;
                            break;
                        case 4: // Ziel
                            img.Source = new BitmapImage(new Uri("C:\\Users\\yalci\\Downloads\\WPF Projekt Sahan\\WPF Projekt Sahan\\ProjektGameKatalog Sahan\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Ziel.avif")); 
                            btn.Content = img;
                            break;
                        case 5: // Spieler
                            img.Source = new BitmapImage(new Uri("C:\\Users\\yalci\\Downloads\\WPF Projekt Sahan\\WPF Projekt Sahan\\ProjektGameKatalog Sahan\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\RPG Spieler.PNG")); 
                            btn.Content = img;
                            break;
                        case 6: // Boss
                            img.Source = new BitmapImage(new Uri("C:\\Users\\yalci\\Downloads\\WPF Projekt Sahan\\WPF Projekt Sahan\\ProjektGameKatalog Sahan\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\RPG Gegner.PNG")); 
                            btn.Content = img;
                            break;
                        case 7: // Heilfeld
                            img.Source = new BitmapImage(new Uri("C:\\Users\\yalci\\Downloads\\WPF Projekt Sahan\\WPF Projekt Sahan\\ProjektGameKatalog Sahan\\ProjektGameKatalog\\ProjektGameKatalog\\Images\\Heal.PNG")); 
                            btn.Content = img;
                            break;
                    }

                    MapGrid.Children.Add(btn);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int dx = 0, dy = 0;
            if (e.Key == Key.Up) dy = -1;
            else if (e.Key == Key.Down) dy = 1;
            else if (e.Key == Key.Left) dx = -1;
            else if (e.Key == Key.Right) dx = 1;

            MovePlayer(dx, dy);
        }

        private void MovePlayer(int dx, int dy)
        {
            int newX = playerX + dx;
            int newY = playerY + dy;

            if (newX < 0 || newY < 0 || newX >= 20 || newY >= 20)
                return;

            if (map[newY, newX] == 1)
            {
                MessageBox.Show("Hier ist eine Wand. Du kannst nicht weiter.");
                return;
            }

            switch (map[newY, newX])
            {
                case 0: // leer
                    MoveTo(newX, newY);
                    break;

                case 2: // Gegner
                    FightEnemy(false);
                    MoveTo(newX, newY);
                    break;

                case 3: // Fragezeichen-Event
                    HandleQuestionMarkEvent(newX, newY);
                    MoveTo(newX, newY);
                    break;

                case 4: // Ziel
                    SaveGoldToDatabase(); // Speichere Gold beim Erreichen des Ziels
                    MessageBoxResult res = MessageBox.Show($"Du hast das Ziel erreicht!\nGold: {playerGold}, HP: {playerHp}\nNeustarten?", "Gewonnen", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        InitializeMap();
                        DrawMap();
                        UpdateStatus();
                        // Gold und HP werden hier nicht zurückgesetzt
                    }
                    else
                    {
                        // Zum Menü zurückkehren und UserID übergeben
                        RPGMenu menuWindow = new RPGMenu(_loggedInUsername, _loggedInUserID);
                        menuWindow.Show();
                        this.Close();
                    }
                    break;

                case 6: // Boss
                    FightEnemy(true);
                    MoveTo(newX, newY);
                    break;

                case 7: // Heilfeld
                    playerHp += 50;
                    MessageBox.Show("Du findest ein Heilfeld und bekommst 50 HP dazu!");
                    map[newY, newX] = 0; // Feld danach leer
                    MoveTo(newX, newY);
                    break;
            }

            UpdateStatus();

            if (playerHp <= 0)
            {
                SaveGoldToDatabase(); // Hinzugefügt: Speichere Gold beim Tod des Spielers
                MessageBoxResult res = MessageBox.Show($"Du bist gestorben!\nGold: {playerGold}, HP: {playerHp}\nNeustarten?", "Verloren", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    InitializeMap(); // Karte neu initialisieren
                    DrawMap();       // Karte neu zeichnen
                    UpdateStatus();  // Status aktualisieren (HP/Gold bleiben)
                }
                else
                {
                    // Zum Menü zurückkehren und UserID übergeben
                    RPGMenu menuWindow = new RPGMenu(_loggedInUsername, _loggedInUserID);
                    menuWindow.Show();
                    this.Close();
                }
            }
        }

        private void FightEnemy(bool isBoss)
        {
            int damage = isBoss ? rnd.Next(15, 31) : rnd.Next(5, 16);
            int gold = isBoss ? rnd.Next(100, 201) : rnd.Next(10, 51);

            playerHp -= damage;
            playerGold += gold;

            string name = isBoss ? "Boss" : "Gegner";
            MessageBox.Show($"Du kämpfst gegen einen {name}!\nVerlierst {damage} HP, bekommst {gold} Gold.");
        }

        private void HandleQuestionMarkEvent(int x, int y)
        {
            int choice = rnd.Next(4);
            switch (choice)
            {
                case 0: // Gold
                    int goldFound = rnd.Next(20, 61);
                    playerGold += goldFound;
                    MessageBox.Show($"Du findest eine Schatztruhe und erhältst {goldFound} Gold!");
                    break;
                case 1: // Buff (Heilung)
                    int heal = rnd.Next(10, 31);
                    playerHp += heal;
                    MessageBox.Show($"Du findest einen Heiltrank und bekommst {heal} HP dazu!");
                    break;
                case 2: // Debuff (Schaden)
                    int damage = rnd.Next(5, 16);
                    playerHp -= damage;
                    MessageBox.Show($"Eine Falle wurde ausgelöst! Du verlierst {damage} HP!");
                    break;
                case 3: // Gegnerkampf
                    FightEnemy(false);
                    break;
            }

            map[y, x] = 0; // Event weg nach Benutzung
        }

        private void MoveTo(int newX, int newY)
        {
            map[playerY, playerX] = 0;
            playerX = newX;
            playerY = newY;
            map[playerY, playerX] = 5;
            DrawMap();
        }

        private void UpdateStatus()
        {
            HpText.Text = $"HP: {playerHp}";
            GoldText.Text = $"Gold: {playerGold}";
        }

        /// Speichert das gesammelte Gold des aktuellen Benutzers in der Datenbank.
        /// Aktualisiert nur, wenn der neue Goldwert höher ist.
        private void SaveGoldToDatabase()
        {
            string userID = _loggedInUserID;
            int goldRPGInt = playerGold; // Das gesammelte Gold

            if (string.IsNullOrEmpty(userID))
            {
                MessageBox.Show("Fehler: Benutzer-ID nicht verfügbar. Gold kann nicht gespeichert werden.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(@"Server=LAPTOP-425O6GTS\SQLEXPRESS;Database=SpielKatalog;Trusted_Connection=True;"))
                {
                    connection.Open();

                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Highscore WHERE UserID = @UserID", connection);
                    checkCmd.Parameters.AddWithValue("@UserID", userID);
                    int exists = (int)checkCmd.ExecuteScalar();

                    SqlCommand cmd;
                    if (exists == 0)
                    {
                        // Füge einen neuen Highscore-Eintrag ein
                        cmd = new SqlCommand(@"INSERT INTO Highscore (UserID, HighscoreSpace, HighscorePacMan, GoldRPG)
                                               VALUES (@UserID, 0, 0, @GoldRPG)", connection);
                        cmd.Parameters.AddWithValue("@UserID", userID);
                        cmd.Parameters.AddWithValue("@GoldRPG", goldRPGInt);
                    }
                    else
                    {
                        // Hole den aktuellen GoldRPG Wert
                        SqlCommand getOldGoldCmd = new SqlCommand("SELECT GoldRPG FROM Highscore WHERE UserID = @UserID", connection);
                        getOldGoldCmd.Parameters.AddWithValue("@UserID", userID);
                        object oldGoldObj = getOldGoldCmd.ExecuteScalar();
                        int oldGold = (oldGoldObj != DBNull.Value) ? Convert.ToInt32(oldGoldObj) : 0;

                        // Aktualisiere nur, wenn der neue Goldwert höher ist
                        if (goldRPGInt > oldGold)
                        {
                            cmd = new SqlCommand(@"UPDATE Highscore SET GoldRPG = @GoldRPG
                                                   WHERE UserID = @UserID", connection);
                            cmd.Parameters.AddWithValue("@UserID", userID);
                            cmd.Parameters.AddWithValue("@GoldRPG", goldRPGInt);
                        }
                        else
                        {
                            MessageBox.Show("RPG Gold wurde nicht aktualisiert, da der aktuelle Goldwert nicht höher ist.");
                            return; // Beende die Methode hier
                        }
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("RPG Gold erfolgreich gespeichert/aktualisiert!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Speichern des RPG Goldes: " + ex.Message);
            }
        }
    }
}
