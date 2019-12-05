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
        public int kivalasztottTablaID;
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
                                    hossz INTEGER NOT NULL,
                                    autoinc BOOLEAN,
                                    prikey BOOLEAN,
                                    tablaid INTEGER NOT NULL
                                    );";
            command.ExecuteNonQuery();
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
            var regexItem = new Regex("^[a-z_]*$");
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
            var regexItem = new Regex("^[a-z_]*$");
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
                    string nev = reader.GetString(1);
                    var megadottTablaNev = tablaNeveTextBox.Text.ToUpper();
                    if (nev.Equals(megadottTablaNev))
                    {
                        MessageBox.Show("Ilyen tábla már létre lett hozva egyszer");
                    }
                }
                */
                cmd.CommandText = "INSERT INTO tablak(tablanev) VALUES (@tablanev)";
                cmd.Parameters.AddWithValue("@tablanev", tablaNeveTextBox.Text);
                cmd.ExecuteNonQuery();

                TablakListazasa();
                tablaNeveTextBox.Clear();
                tablaNeveAlahuzasPanel.BackColor = Color.Black;
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

        private void TablakListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tablakListBox.SelectedIndex != -1)
            {
                Tablak tabla = (Tablak)tablakListBox.SelectedItem;
                kivalasztottTablaID = tabla.Id;
                oszlopNevTextBox.Enabled = true;
                oszlopNeveAlahuzasPanel.BackColor = Color.Black;
                OszlopListazas(kivalasztottTablaID);
            }
        }

        private void OszlopNevTextBox_TextChanged(object sender, EventArgs e)
        {
            var regexItem = new Regex("^[a-z_]*$");
            if (regexItem.IsMatch(oszlopNevTextBox.Text) && oszlopNevTextBox.Text.Length > 3 && oszlopNevTextBox.Text.Length < 128)
            {
                oszlopNeveAlahuzasPanel.BackColor = Color.Green;
                oszlopKiterjesztesComboBox.Enabled = true;
            }
            else
            {
                oszlopNeveAlahuzasPanel.BackColor = Color.Red;
            }
        }

        private void OszlopKiterjesztesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            oszlopHozzaadasButton.Enabled = true;
            switch (oszlopKiterjesztesComboBox.Text)
            {
                case "INTEGER":
                    autoIncrementCheckBox.Enabled = true;
                    primaryKeyCheckBox.Enabled = true;
                    oszlopHosszNumericUpDown.Maximum = 64;
                    oszlopHosszNumericUpDown.Enabled = true;
                    break;
                case "BOOLEAN":
                    oszlopHosszNumericUpDown.Value = 1;
                    oszlopHosszNumericUpDown.Enabled = false;
                    autoIncrementCheckBox.Checked = false;
                    primaryKeyCheckBox.Checked = false;
                    autoIncrementCheckBox.Enabled = false;
                    primaryKeyCheckBox.Enabled = false;
                    break;
                case "VARCHAR":
                    oszlopHosszNumericUpDown.Maximum = 128;
                    autoIncrementCheckBox.Checked = false;
                    primaryKeyCheckBox.Checked = false;
                    autoIncrementCheckBox.Enabled = false;
                    primaryKeyCheckBox.Enabled = false;
                    oszlopHosszNumericUpDown.Enabled = true;
                    break;
                case "TEXT":
                    autoIncrementCheckBox.Checked = false;
                    primaryKeyCheckBox.Checked = false;
                    autoIncrementCheckBox.Enabled = false;
                    primaryKeyCheckBox.Enabled = false;
                    oszlopHosszNumericUpDown.Enabled = true;
                    break;
            }
        }

        private void OszlopHozzaadasButton_Click(object sender, EventArgs e)
        {
            if (oszlopNeveAlahuzasPanel.BackColor == Color.Green && kivalasztottTablaID >= 0)
            {

                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO oszlopok(oszlopnev, kiterjesztes, hossz, autoinc, prikey, tablaid) VALUES (@oszlopnev, @kiterjesztes, @hossz, @autoinc, @prikey, @tablaid)";
                cmd.Parameters.AddWithValue("@oszlopnev", oszlopNevTextBox.Text);
                cmd.Parameters.AddWithValue("@kiterjesztes", oszlopKiterjesztesComboBox.SelectedItem);
                cmd.Parameters.AddWithValue("@hossz", oszlopHosszNumericUpDown.Value);
                cmd.Parameters.AddWithValue("@autoinc", autoIncrementCheckBox.Checked);
                cmd.Parameters.AddWithValue("@prikey", primaryKeyCheckBox.Checked);
                cmd.Parameters.AddWithValue("@tablaid", kivalasztottTablaID);
                cmd.ExecuteNonQuery();

                OszlopListazas(kivalasztottTablaID);
                OszlopHozzaadReset();
            }
            else
            {
                MessageBox.Show("Kérjük válasszon ki egy táblát a listából");
            }
        }

        public void OszlopListazas(int index) {
            OszlopokListBox.Items.Clear();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, oszlopnev, kiterjesztes, hossz, autoinc, prikey, tablaid FROM oszlopok WHERE tablaid = @tablaid";
            cmd.Parameters.AddWithValue("@tablaid", kivalasztottTablaID);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var oszlopnev = reader.GetString(1);
                    var kiterjesztes = reader.GetString(2);
                    var hossz = reader.GetInt32(3);
                    var autoinc = reader.GetBoolean(4);
                    var prikey = reader.GetBoolean(5);
                    var tablaid = reader.GetInt32(6);
                    var oszlop = new Oszlopok(id, oszlopnev, kiterjesztes, hossz, autoinc, prikey, tablaid);
                    OszlopokListBox.Items.Add(oszlop);
                }
            }
        }
        public void OszlopHozzaadReset() {
            oszlopNevTextBox.Clear();
            oszlopNeveAlahuzasPanel.BackColor = Color.Black;
            oszlopKiterjesztesComboBox.Text = " ";
            oszlopHosszNumericUpDown.Value = 1;
            oszlopKiterjesztesComboBox.Enabled = false;
            oszlopHosszNumericUpDown.Enabled = false;
            autoIncrementCheckBox.Checked = false;
            primaryKeyCheckBox.Checked = false;
            autoIncrementCheckBox.Enabled = false;
            primaryKeyCheckBox.Enabled = false;
            oszlopHozzaadasButton.Enabled = false;
        }


    }
}
