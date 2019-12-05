using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PHPSQLSeged
{
    public partial class PHPSQLSeged : Form
    {
        SQLiteConnection conn;
        public PHPSQLSeged()
        {
            InitializeComponent();
            sqlPanel.Visible = false;

            conn = new SQLiteConnection("Data Source = sql.db");
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS tablak(
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    tablanev VARCHAR(128) NOT NULL
                                    );
                                    CREATE TABLE IF NOT EXISTS oszlopok(
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    oszlopnev VARCHAR(128) NOT NULL,
                                    kiterjesztes VARCHAR(128) NOT NULL,
                                    autoinc BOOLEAN,
                                    prikey BOOLEAN,
                                    tablaid INTEGER NOT NULL
                                    );";
            command.ExecuteNonQuery();
            TablakListazasa();
        }
        private void KezdolapButton_Click(object sender, EventArgs e)
        {
            kezdolapJelolo.Visible = true;
            sqlJelolo.Visible = false;
            phpJelolo.Visible = false;
            mentesJelolo.Visible = false;
            kezdolapPanel.Visible = true;
            sqlPanel.Visible = false;
        }

        private void SqlButton_Click(object sender, EventArgs e)
        {
            kezdolapJelolo.Visible = false;
            sqlJelolo.Visible = true;
            phpJelolo.Visible = false;
            mentesJelolo.Visible = false;
            kezdolapPanel.Visible = false;
            sqlPanel.Visible = true;

        }

        private void PhpButton_Click(object sender, EventArgs e)
        {
            kezdolapJelolo.Visible = false;
            sqlJelolo.Visible = false;
            phpJelolo.Visible = true;
            mentesJelolo.Visible = false;
        }

        private void MentesButton_Click(object sender, EventArgs e)
        {
            kezdolapJelolo.Visible = false;
            sqlJelolo.Visible = false;
            phpJelolo.Visible = false;
            mentesJelolo.Visible = true;
        }

        

        private void AdatbazisNeveTextBox_TextChanged(object sender, EventArgs e)
        {
            var regexItem = new Regex("^[a-zA-Z_]*$");
            if (regexItem.IsMatch(adatbazisNeveTextBox.Text) && adatbazisNeveTextBox.Text.Length > 3 && adatbazisNeveTextBox.Text.Length < 128)
            {
                adatbazisNevAlahuzasPanel.BackColor = Color.Green;
                tablaNeveTextBox.Enabled = true;
                tablaNeveAlahuzasPanel.BackColor = Color.Black;
            }
            else
            {
                adatbazisNevAlahuzasPanel.BackColor = Color.Red;
                tablaNeveTextBox.Enabled = false;
                tablaNeveAlahuzasPanel.BackColor = Color.Gray;
            }
        }

        private void TablaNeveTextBox_TextChanged(object sender, EventArgs e)
        {
            var regexItem = new Regex("^[a-zA-Z_]*$");
            if (regexItem.IsMatch(tablaNeveTextBox.Text) && tablaNeveTextBox.Text.Length > 3 && tablaNeveTextBox.Text.Length < 128)
            {
                tablaNeveAlahuzasPanel.BackColor = Color.Green;
            }
            else
            {
                tablaNeveAlahuzasPanel.BackColor = Color.Red;
            }
        }
        public void TablakListazasa()
        {
            tablakListBox.Items.Clear();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, tablanev FROM tablak";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string tablanev = reader.GetString(1);
                    var tabla = new Tablak(id, tablanev);
                    tablakListBox.Items.Add(tabla);
                }
            }
        }
        private void TablaNeveTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (tablaNeveAlahuzasPanel.BackColor == Color.Green && e.KeyCode == Keys.Enter)
            {
                var cmd = conn.CreateCommand();
                /*
                cmd.CommandText = "SELECT id, tablanev FROM tablak";
                
                using (var reader = cmd.ExecuteReader())
                {
                    string nev = reader.GetString(1).ToUpper();
                    var megadottTablaNev = tablaNeveTextBox.Text.ToUpper();
                    if (nev.Equals(megadottTablaNev))
                    {
                        MessageBox.Show("Ilyen tábla már létre lett hozva egyszer");
                    }
                }
                */
                cmd.CommandText = "INSERT INTO tablak(id, tablanev) VALUES (null, @tablanev)";
                cmd.Parameters.AddWithValue("@tablanev", tablaNeveTextBox.Text);
                cmd.ExecuteNonQuery();

                TablakListazasa();
                tablaNeveTextBox.Clear();
            }
        }

        private void KilepesButton_Click_1(object sender, EventArgs e)
        {
            conn.Close();
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            File.Delete("sql.db");
            Application.Exit();

        }

        
    }
}
