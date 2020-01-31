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
        public int kivalasztottOszlopID;
        public bool kezdolap, sql, php, mentes = false;
        public PHPSQLSeged()
        {
            InitializeComponent();


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
            Oldalvaltas(1);
        }

        private void SqlButton_Click(object sender, EventArgs e)
        {
            Oldalvaltas(2);
        }

        private void PhpButton_Click(object sender, EventArgs e)
        {
            Oldalvaltas(3);
        }

        private void MentesButton_Click(object sender, EventArgs e)
        {
            Oldalvaltas(4);
        }

        public void Oldalvaltas(int oldal) {
            kezdolap = false;
            sql = false;
            php = false;
            mentes = false;
            switch (oldal) {
                case 1: kezdolap = true; break;
                case 2: sql = true;  break;
                case 3: php = true;  break;
                case 4: mentes = true; break;
            }
            kezdolapJelolo.Visible = kezdolap;
            kezdolapPanel.Visible = kezdolap;
            sqlJelolo.Visible = sql;
            sqlPanel.Visible = sql;
            phpJelolo.Visible = php;
            phpPanel.Visible = php;
            mentesJelolo.Visible = mentes;
            /*mentesPanel.Visible = mentes;*/
        }


        private void AdatbazisNeveTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SzovegEllenorzes(adatbazisNeveTextBox.Text))
            {
                adatbazisNevAlahuzasPanel.BackColor = Color.Green;
                tablaNeveTextBox.Enabled = true;
                tablaNeveAlahuzasPanel.BackColor = Color.Black;
                tablaHozzaadasButton.Enabled = true;
                tablakListBox.Enabled = true;
            }
            else
            {
                adatbazisNevAlahuzasPanel.BackColor = Color.Red;
                tablaNeveTextBox.Enabled = false;
                tablaNeveAlahuzasPanel.BackColor = Color.Gray;
                tablaHozzaadasButton.Enabled = false;
                tablakListBox.Enabled = false;
                oszlopHozzaadasGroupBox.Enabled = false;
            }
        }

        private void TablaNeveTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SzovegEllenorzes(tablaNeveTextBox.Text))
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
            if (e.KeyCode == Keys.Enter) {
                TablaHozzaAdas();
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
                var nev = tabla.Tablanev;
                oszlopNevTextBox.Enabled = true;
                oszlopNeveAlahuzasPanel.BackColor = Color.Black;
                OszlopListazas(kivalasztottTablaID);
                tablaTorlesButton.Enabled = true;
                tablakModositasButton.Enabled = true;
                oszlopHozzaadasGroupBox.Enabled = true;
                tablaModositottNeveTextBox.Text = nev;
            }
        }

        private void OszlopNevTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SzovegEllenorzes(oszlopNevTextBox.Text))
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
            oszlopKiterjesztesComboBox.SelectedIndex = -1;
            oszlopHosszNumericUpDown.Value = 1;
            oszlopKiterjesztesComboBox.Enabled = false;
            oszlopHosszNumericUpDown.Enabled = false;
            autoIncrementCheckBox.Checked = false;
            primaryKeyCheckBox.Checked = false;
            autoIncrementCheckBox.Enabled = false;
            primaryKeyCheckBox.Enabled = false;
            oszlopHozzaadasButton.Enabled = false;
        }

        private void TablaHozzaadasButton_Click(object sender, EventArgs e)
        {
            TablaHozzaAdas();
        }
        public void TablaHozzaAdas() {
            if (tablaNeveAlahuzasPanel.BackColor == Color.Green)
            {
                var cmd = conn.CreateCommand();

                /*cmd.CommandText = "SELECT id, tablanev FROM tablak";
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

        private void TablaTorlesButton_Click(object sender, EventArgs e)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM tablak WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", kivalasztottTablaID);
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DELETE FROM oszlopok WHERE tablaid = @tablaid";
            cmd.Parameters.AddWithValue("@tablaid", kivalasztottTablaID);
            cmd.ExecuteNonQuery();
            TablakListazasa();
            OszlopListazas(-1);
        }

        private void TablakModositasButton_Click(object sender, EventArgs e)
        {
            tablaHozzaadasPanel.Visible = false;
            tablaModositasPanel.Visible = true;
            tablaModositottNeveTextBox.Enabled = true;
            tablaNeveModositasVegrehajtasButton.Enabled = true;
            tablaModositottNeveEllenorzoPanel.Enabled = true;
            tablaModositottNeveEllenorzoPanel.BackColor = Color.Black;
        }

        private void TablaModositottNeveTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SzovegEllenorzes(tablaModositottNeveTextBox.Text))
            {
                tablaModositottNeveEllenorzoPanel.BackColor = Color.Green;
            }
            else
            {
                tablaModositottNeveEllenorzoPanel.BackColor = Color.Red;
            }
        }

        private void TablaNeveModositasVegrehajtasButton_Click(object sender, EventArgs e)
        {
            if (tablaModositottNeveEllenorzoPanel.BackColor == Color.Green)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE tablak SET tablanev = @tablanev WHERE id = @id";
                cmd.Parameters.AddWithValue("@tablanev", tablaModositottNeveTextBox.Text);
                cmd.Parameters.AddWithValue("@id", kivalasztottTablaID);
                cmd.ExecuteNonQuery();
                TablakListazasa();
                tablaModositasPanel.Visible = false;
                tablaModositottNeveTextBox.Clear();
                tablaHozzaadasPanel.Visible = true;
            }
            else {
                MessageBox.Show("Nem megfelelő adatot adott meg próbálja újra");
            }
        }

        private void OszlopokListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OszlopokListBox.SelectedIndex != -1)
            if (OszlopokListBox.SelectedIndex != -1)
                {
                oszlopTorlesButton.Enabled = true;
                oszlopModositasButton.Enabled = true;
                var oszlop = (Oszlopok)OszlopokListBox.SelectedItem;
                kivalasztottOszlopID = oszlop.Id;
            }
            else{
                MessageBox.Show("Kérjük válasszon ki egy érvényes oszlopot");
            }
        }

        private void OszlopTorlesButton_Click(object sender, EventArgs e)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM oszlopok WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", kivalasztottOszlopID);
            cmd.ExecuteNonQuery();
            OszlopListazas(kivalasztottTablaID);
        }

        private void TablaModositottNeveTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (tablaModositottNeveEllenorzoPanel.BackColor == Color.Green && e.KeyCode == Keys.Enter)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE tablak SET tablanev = @tablanev WHERE id = @id";
                cmd.Parameters.AddWithValue("@tablanev", tablaModositottNeveTextBox.Text);
                cmd.Parameters.AddWithValue("@id", kivalasztottTablaID);
                cmd.ExecuteNonQuery();
                TablakListazasa();
                tablaModositasPanel.Visible = false;
                tablaModositottNeveTextBox.Clear();
                tablaHozzaadasPanel.Visible = true;
            }
        }

        private void OszlopModositasButton_Click(object sender, EventArgs e)
        {
            oszlopHozzaadasGroupBox.Visible = false;
            oszlopModositasGroupBox.Visible = true;
            var oszlop = (Oszlopok)OszlopokListBox.SelectedItem;
            oszlopModositottNeveTextBox.Text = oszlop.OszlopNeve;
            oszlopModositottKiterjesztésComboBox.SelectedItem = oszlop.Kiterjesztes;
            oszlopModositottHosszaNumericUpDown.Value = oszlop.Hossz;
            modositottOszlopNeveEllenorzoPanel.BackColor = Color.Black;
            if (oszlop.Autoincrement)
            {
                modositottAutoIncrementCheckBox.Checked = true;
            }
            if (oszlop.PrimaryKey)
            {
                modositottPrimaryKeyCheckBox.Checked = true;
            }
        }

        private void OszlopModositottNeveTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SzovegEllenorzes(oszlopModositottNeveTextBox.Text))
            {
                modositottOszlopNeveEllenorzoPanel.BackColor = Color.Green;
                oszlopModositottKiterjesztésComboBox.Enabled = true;
            }
            else
            {
                modositottOszlopNeveEllenorzoPanel.BackColor = Color.Red;
            }
        }

        private void OszlopModositottKiterjesztésComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            oszlopModositasokVegrehajtasaButton.Enabled = true;
            switch (oszlopModositottKiterjesztésComboBox.Text)
            {
                case "INTEGER":
                    modositottAutoIncrementCheckBox.Enabled = true;
                    modositottPrimaryKeyCheckBox.Enabled = true;
                    oszlopModositottHosszaNumericUpDown.Maximum = 64;
                    oszlopModositottHosszaNumericUpDown.Enabled = true;
                    break;
                case "BOOLEAN":
                    oszlopModositottHosszaNumericUpDown.Value = 1;
                    oszlopModositottHosszaNumericUpDown.Enabled = false;
                    modositottAutoIncrementCheckBox.Checked = false;
                    modositottPrimaryKeyCheckBox.Checked = false;
                    modositottAutoIncrementCheckBox.Enabled = false;
                    modositottPrimaryKeyCheckBox.Enabled = false;
                    break;
                case "VARCHAR":
                    oszlopModositottHosszaNumericUpDown.Maximum = 128;
                    modositottAutoIncrementCheckBox.Checked = false;
                    modositottPrimaryKeyCheckBox.Checked = false;
                    modositottAutoIncrementCheckBox.Enabled = false;
                    modositottPrimaryKeyCheckBox.Enabled = false;
                    oszlopModositottHosszaNumericUpDown.Enabled = true;
                    break;
                case "TEXT":
                    modositottAutoIncrementCheckBox.Checked = false;
                    modositottPrimaryKeyCheckBox.Checked = false;
                    modositottAutoIncrementCheckBox.Enabled = false;
                    modositottPrimaryKeyCheckBox.Enabled = false;
                    oszlopModositottHosszaNumericUpDown.Enabled = true;
                    break;
            }
        }

        private void OszlopModositasokVegrehajtasaButton_Click(object sender, EventArgs e)
        {
            if (modositottOszlopNeveEllenorzoPanel.BackColor == Color.Green || modositottOszlopNeveEllenorzoPanel.BackColor == Color.Black)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE oszlopok SET oszlopnev = @oszlopnev, kiterjesztes = @kiterjesztes, hossz = @hossz, autoinc = @autoinc, prikey = @prikey WHERE id = @id";
                cmd.Parameters.AddWithValue("@oszlopnev", oszlopModositottNeveTextBox.Text);
                cmd.Parameters.AddWithValue("@kiterjesztes", oszlopModositottKiterjesztésComboBox.SelectedItem);
                cmd.Parameters.AddWithValue("@hossz", oszlopModositottHosszaNumericUpDown.Value);
                cmd.Parameters.AddWithValue("@autoinc", modositottAutoIncrementCheckBox.Checked);
                cmd.Parameters.AddWithValue("@prikey", modositottPrimaryKeyCheckBox.Checked);
                cmd.Parameters.AddWithValue("@id", kivalasztottOszlopID);
                cmd.ExecuteNonQuery();
                OszlopListazas(kivalasztottTablaID);
                oszlopModositasGroupBox.Visible = false;
                oszlopHozzaadasGroupBox.Visible = true;
            }
            else {
                MessageBox.Show("Kérjük érvényes adatokkat adjon meg!");
            }
        }

        private bool SzovegEllenorzes(string szoveg)
        {
            var regexItem = new Regex("^[a-z_]*$");
            if (regexItem.IsMatch(szoveg) && szoveg.Length >= 2 && szoveg.Length < 128)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}