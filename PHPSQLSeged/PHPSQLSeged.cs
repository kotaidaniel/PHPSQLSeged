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
        public int kivalasztottPHPTablaID;
        public bool kezdolap, sql, php, mentes = false;
        public PHPSQLSeged()
        {
            InitializeComponent();


            conn = new SQLiteConnection("Data Source = sql.db");
            conn.Open();
            var command = conn.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS tablak(
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    tablanev VARCHAR(128) NOT NULL,
                                    cmd_select BOOLEAN,
                                    cmd_insert BOOLEAN,
                                    cmd_delete BOOLEAN,
                                    cmd_update BOOLEAN
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

            ideiglenesMentes();
            Betoltes();
            sqlMentes();
            phpMentes();
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

        public void Oldalvaltas(int oldal)
        {
            kezdolap = false;
            sql = false;
            php = false;
            mentes = false;
            switch (oldal)
            {
                case 1: kezdolap = true; break;
                case 2: sql = true; break;
                case 3: php = true; break;
                case 4: mentes = true; break;
            }
            kezdolapJelolo.Visible = kezdolap;
            kezdolapPanel.Visible = kezdolap;
            sqlJelolo.Visible = sql;
            sqlPanel.Visible = sql;
            phpJelolo.Visible = php;
            phpPanel.Visible = php;
            mentesJelolo.Visible = mentes;
            mentesPanel.Visible = mentes;
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
            phpTablakListBox.Items.Clear();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM tablak";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string tablanev = reader.GetString(1);
                    var tabla = new Tablak(id, tablanev);
                    tablakListBox.Items.Add(tabla);
                    phpTablakListBox.Items.Add(tabla);
                }
            }
        }
        private void TablaNeveTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TablaHozzaAdas();
            }
        }

        private void KilepesButton_Click_1(object sender, EventArgs e)
        {
            string uzenet = "Biztosan ki szeretne lépni az alkalmazásból? Kilépésnél a nem mentett munkák törlésre kerülnek!";
            string cim = "Kilépés";
            var ablak = MessageBox.Show(uzenet, cim, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ablak == DialogResult.Yes)
            {
                conn.Close();
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                File.Delete("sql.db");
                Application.Exit();
            }
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

        public void OszlopListazas(int index)
        {
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
        public void OszlopHozzaadReset()
        {
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
        public void TablaHozzaAdas()
        {
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
                cmd.CommandText = "INSERT INTO tablak(id, tablanev, cmd_select, cmd_insert, cmd_delete, cmd_update) VALUES (NULL, @tablanev, 0, 0, 0, 0)";
                cmd.Parameters.AddWithValue("@tablanev", tablaNeveTextBox.Text);
                cmd.ExecuteNonQuery();

                TablakListazasa();
                tablaNeveTextBox.Clear();
                tablaNeveAlahuzasPanel.BackColor = Color.Black;
            }
        }

        private void TablaTorlesButton_Click(object sender, EventArgs e)
        {
            if (DeleteMessageBox())
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
            else
            {
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
                else
                {
                    MessageBox.Show("Kérjük válasszon ki egy érvényes oszlopot");
                }
        }

        private void OszlopTorlesButton_Click(object sender, EventArgs e)
        {
            if (DeleteMessageBox())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM oszlopok WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", kivalasztottOszlopID);
                cmd.ExecuteNonQuery();
                OszlopListazas(kivalasztottTablaID);
            }
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

        private void PhpTablakListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (phpTablakListBox.SelectedIndex != -1)
            {
                selectCheckBox.Enabled = true;
                insertCheckBox.Enabled = true;
                deleteCheckBox.Enabled = true;
                updateCheckBox.Enabled = true;
                var tabla = (Tablak)phpTablakListBox.SelectedItem;
                kivalasztottPHPTablaID = tabla.Id;

                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT cmd_select FROM tablak WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", kivalasztottPHPTablaID);
                var select = (bool)cmd.ExecuteScalar();
                cmd.CommandText = "SELECT cmd_insert FROM tablak WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", kivalasztottPHPTablaID);
                var insert = (bool)cmd.ExecuteScalar();
                cmd.CommandText = "SELECT cmd_delete FROM tablak WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", kivalasztottPHPTablaID);
                var delete = (bool)cmd.ExecuteScalar();
                cmd.CommandText = "SELECT cmd_update FROM tablak WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", kivalasztottPHPTablaID);
                var update = (bool)cmd.ExecuteScalar();

                switch (select)
                {
                    case true: selectCheckBox.Checked = true; break;
                    default:
                        selectCheckBox.Checked = false;
                        break;
                }
                switch (insert)
                {
                    case true: insertCheckBox.Checked = true; break;
                    default:
                        insertCheckBox.Checked = false;
                        break;
                }
                switch (delete)
                {
                    case true: deleteCheckBox.Checked = true; break;
                    default:
                        deleteCheckBox.Checked = false;
                        break;
                }
                switch (update)
                {
                    case true: updateCheckBox.Checked = true; break;
                    default:
                        updateCheckBox.Checked = false;
                        break;
                }
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
                    oszlopModositottHosszaNumericUpDown.Maximum = 7;
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
                    oszlopModositottHosszaNumericUpDown.Maximum = 255;
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
        private void SelectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBoxEsemeny(selectCheckBox, selectCheckBox.Checked, "cmd_select");
        }

        private void InsertCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBoxEsemeny(insertCheckBox, insertCheckBox.Checked, "cmd_insert");
        }

        private void DeleteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBoxEsemeny(deleteCheckBox, deleteCheckBox.Checked, "cmd_delete");
        }

        private void UpdateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBoxEsemeny(updateCheckBox, updateCheckBox.Checked, "cmd_update");
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
            else
            {
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

        private void IdeiglenesMentesButton_Click(object sender, EventArgs e)
        {
            ideiglenesSaveFileDialog.ShowDialog();
        }

        private void CheckBoxEsemeny(CheckBox checkbox, bool valasz, string command)
        {
            if (valasz)
            {
                checkbox.BackColor = Color.Green;
                var cmd = conn.CreateCommand();
                switch (command)
                {
                    case "cmd_select":
                        cmd.CommandText = "UPDATE tablak SET cmd_select = true WHERE id = @id";
                        break;
                    case "cmd_insert":
                        cmd.CommandText = "UPDATE tablak SET cmd_insert = true WHERE id = @id";
                        break;
                    case "cmd_delete":
                        cmd.CommandText = "UPDATE tablak SET cmd_delete = true WHERE id = @id";
                        break;
                    case "cmd_update":
                        cmd.CommandText = "UPDATE tablak SET cmd_update = true WHERE id = @id";
                        break;
                }
                cmd.Parameters.AddWithValue("@id", kivalasztottPHPTablaID);
                cmd.ExecuteNonQuery();

            }
            else
            {
                checkbox.BackColor = Color.Red;
                var cmd = conn.CreateCommand();
                switch (command)
                {
                    case "cmd_select":
                        cmd.CommandText = "UPDATE tablak SET cmd_select = false WHERE id = @id";
                        break;
                    case "cmd_insert":
                        cmd.CommandText = "UPDATE tablak SET cmd_insert = false WHERE id = @id";
                        break;
                    case "cmd_delete":
                        cmd.CommandText = "UPDATE tablak SET cmd_delete = false WHERE id = @id";
                        break;
                    case "cmd_update":
                        cmd.CommandText = "UPDATE tablak SET cmd_update = false WHERE id = @id";
                        break;
                }
                cmd.Parameters.AddWithValue("@id", kivalasztottPHPTablaID);
                cmd.ExecuteNonQuery();
            }
        }

        private void BetoltesButton_Click(object sender, EventArgs e)
        {
            string uzenet = "Biztosan be szeretné tölteni a fájlt? Betöltésnél a nem mentett munkák törlésre kerülnek!";
            string cim = "Betöltés";
            var ablak = MessageBox.Show(uzenet, cim, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ablak == DialogResult.Yes)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM tablak WHERE 1";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM oszlopok WHERE 1";
                cmd.ExecuteNonQuery();
                betoltesOpenFileDialog.ShowDialog();
            }
        }

        public bool DeleteMessageBox()
        {
            bool valasz = false;
            string uzenet = "Biztos törölni szeretné a kiválaszott sort?";
            string cim = "Sor törlés";
            var ablak = MessageBox.Show(uzenet, cim, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ablak == DialogResult.Yes)
            {
                valasz = true;
            }
            return valasz;


        }
        public void ideiglenesMentes()
        {
            ideiglenesSaveFileDialog.FileOk += (senderFile, eFile) =>
            {
                try
                {
                    string fileName = ideiglenesSaveFileDialog.FileName;
                    using (var sw = new StreamWriter(fileName))
                    {
                        string adatbazisneve = "";
                        if (adatbazisNevAlahuzasPanel.BackColor == Color.Green)
                        {
                            adatbazisneve = adatbazisNeveTextBox.Text;
                        }
                        else
                        {
                            MessageBox.Show("Érvénytelen az adatbázis neve! Kérjük változtassa meg!");
                            return;
                        }
                        sw.WriteLine(adatbazisneve);

                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT * FROM tablak";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string tablanev = reader.GetString(1);
                                bool cmd_select = reader.GetBoolean(2);
                                bool cmd_insert = reader.GetBoolean(3);
                                bool cmd_delete = reader.GetBoolean(4);
                                bool cmd_update = reader.GetBoolean(5);
                                sw.WriteLine(id + ";" + tablanev + ";" + cmd_select + ";" + cmd_insert + ";" + cmd_delete + ";" + cmd_update);
                            }
                        }
                        sw.WriteLine("#");
                        cmd.CommandText = "SELECT * FROM oszlopok";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string oszlopnev = reader.GetString(1);
                                string kiterjesztes = reader.GetString(2);
                                int hossz = reader.GetInt32(3);
                                bool autoinc = reader.GetBoolean(4);
                                bool prikey = reader.GetBoolean(5);
                                int tablaid = reader.GetInt32(6);
                                sw.WriteLine(id + ";" + oszlopnev + ";" + kiterjesztes + ";" + hossz + ";" + autoinc + ";" + prikey + ";" + tablaid);
                            }
                        }

                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Hiba, nem sikerült a mentés");
                }

            };
        }

        private void SqlTallozasButton_Click(object sender, EventArgs e)
        {
            sqlSaveFileDialog.ShowDialog();
        }

        public void Betoltes()
        {
            betoltesOpenFileDialog.FileOk += (senderFile, eFile) =>
            {
                try
                {
                    ResetComponents(sqlPanel);
                    ResetComponents(phpPanel);
                    string[] sorok = File.ReadAllLines(betoltesOpenFileDialog.FileName);
                    adatbazisNeveTextBox.Text = sorok[0];
                    int index = 1;
                    while (sorok[index] != "#")
                    {
                        string[] adatok = sorok[index].Split(';');
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "INSERT INTO tablak (id, tablanev, cmd_select, cmd_insert, cmd_delete, cmd_update) " +
                        "VALUES (NULL, @tablanev, @cmd_select, @cmd_insert, @cmd_delete, @cmd_update)";
                        cmd.Parameters.AddWithValue("@tablanev", adatok[1]);
                        cmd.Parameters.AddWithValue("@cmd_select", Convert.ToBoolean(adatok[2]));
                        cmd.Parameters.AddWithValue("@cmd_insert", Convert.ToBoolean(adatok[3]));
                        cmd.Parameters.AddWithValue("@cmd_delete", Convert.ToBoolean(adatok[4]));
                        cmd.Parameters.AddWithValue("@cmd_update", Convert.ToBoolean(adatok[5]));
                        cmd.ExecuteNonQuery();
                        index++;
                    }
                    TablakListazasa();
                    index += 1;
                    while (index != sorok.Length)
                    {
                        string[] adatok2 = sorok[index].Split(';');
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "INSERT INTO oszlopok (id, oszlopnev, kiterjesztes, hossz, autoinc, prikey, tablaid) " +
                        "VALUES (NULL, @oszlopnev, @kiterjesztes, @hossz, @autoinc, @prikey, @tablaid)";
                        cmd.Parameters.AddWithValue("@oszlopnev", adatok2[1]);
                        cmd.Parameters.AddWithValue("@kiterjesztes", adatok2[2]);
                        cmd.Parameters.AddWithValue("@hossz", Convert.ToInt32(adatok2[3]));
                        cmd.Parameters.AddWithValue("@autoinc", Convert.ToBoolean(adatok2[4]));
                        cmd.Parameters.AddWithValue("@prikey", Convert.ToBoolean(adatok2[5]));
                        cmd.Parameters.AddWithValue("@tablaid", Convert.ToInt32(adatok2[6]));
                        cmd.ExecuteNonQuery();
                        index++;

                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Nem sikerült megnyitni a fájlt");
                }
            };
        }
        public void ResetComponents(Panel panel)
        {
            foreach (var control in panel.Controls)
            {
                if (control is TextBox)
                {
                    TextBox textbox = (TextBox)control;
                    textbox.Text = null;
                }
                if (control is CheckBox)
                {
                    CheckBox checkbox = (CheckBox)control;
                    checkbox.Checked = false;
                }
                if (control is ListBox)
                {
                    ListBox listBox = (ListBox)control;
                    listBox.Items.Clear();
                }
            }
            OszlopHozzaadReset();
        }
        public void sqlMentes()
        {
            sqlSaveFileDialog.FileOk += (senderFile, eFile) =>
            {
                try
                {

                    string fileName = sqlSaveFileDialog.FileName;
                    using (var sw = new StreamWriter(fileName))
                    {
                        sw.WriteLine("DROP DATABASE {0};", adatbazisNeveTextBox.Text);
                        sw.WriteLine("CREATE DATABASE {0};", adatbazisNeveTextBox.Text);
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT * FROM tablak";
                        var oszlopKereses_cmd = conn.CreateCommand();
                        oszlopKereses_cmd.CommandText = "SELECT * FROM oszlopok WHERE tablaid = @id";
                        string prikeyoszlopnev = "";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string tablanev = reader.GetString(1);
                                oszlopKereses_cmd.Parameters.AddWithValue("@id", id);
                                sw.WriteLine("CREATE TABLE IF NOT EXISTS {0}.{1} (", adatbazisNeveTextBox.Text, tablanev);
                                int db = 1;
                                bool prikey = false;
                                using (var oszlopReader = oszlopKereses_cmd.ExecuteReader())
                                {
                                    while (oszlopReader.Read())
                                    {
                                        string oszlopnev = oszlopReader.GetString(1);
                                        string kiterjesztes = oszlopReader.GetString(2);
                                        int hossz = oszlopReader.GetInt32(3);
                                        string autoinc = "";
                                        if (oszlopReader.GetBoolean(4))
                                        {
                                            autoinc = " AUTO_INCREMENT";
                                        }
                                        if (oszlopReader.GetBoolean(5))
                                        {
                                            prikey = true;
                                            prikeyoszlopnev = oszlopnev;
                                        }
                                        if (kiterjesztes == "BOOLEAN")
                                        {
                                            sw.WriteLine(oszlopnev + " " + kiterjesztes + ",");

                                        }
                                        else
                                        {
                                            sw.WriteLine(oszlopnev + " " + kiterjesztes + "(" + hossz + ")" + autoinc + ",");

                                        }
                                        db++;
                                    }
                                }
                                if (prikey)
                                {
                                    sw.WriteLine("PRIMARY KEY({0})", prikeyoszlopnev);
                                }
                                sw.WriteLine(");");
                            }
                        }
                    }
                    sqlPathTextBox.Text = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileName(fileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Nem sikerült a fájl mentése");
                }
            };
        }

        private void PhpTallozasButton_Click(object sender, EventArgs e)
        {
            phpSaveFileDialog.ShowDialog();
        }

        public long OszlopMennyiseg(int tablaid)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM oszlopok WHERE tablaid = @id";
            cmd.Parameters.AddWithValue("@id", tablaid);
            long db = (long)cmd.ExecuteScalar();

            return db;
        }

        public void phpMentes()
        {
            phpSaveFileDialog.FileOk += (senderFile, eFile) =>
            {
                try
                {
                    string fileName = phpSaveFileDialog.FileName;
                    using (var sw = new StreamWriter(fileName))
                    {
                        sw.WriteLine("<!DOCTYPE html>"+
                        "\n<body>" +
                        "\n<form method = \"POST\">" + 
                        "\n\tVálassza ki a táblát:" +
                        "\n\t<select name = \"input_tabla\">");
                        for (int i = 0; i < TablakKivalasztasa().Count; i++)
                        {
                            string[] tablaAdatok = TablakKivalasztasa()[i].Split(';');
                            sw.WriteLine("\t\t<option value = \""+tablaAdatok[1]+"\">" + tablaAdatok[1] + "</option>");
                        }
                        sw.WriteLine("<input type = \"hidden\" name = \"action\" value = \"cmd_tabla_kivalasztas\">" +
                                     "\n<button type = \"submit\" > Küldés </button>");
                        sw.WriteLine("\t</select>" +
                            "\n</form>");
                        for (int i = 0; i < TablakKivalasztasa().Count; i++)
                        {
                            string[] tablaadatok = TablakKivalasztasa()[i].Split(';');
                            if (Convert.ToBoolean(tablaadatok[3]))
                            {
                                sw.WriteLine(tablaadatok[1] + ":</br>");
                                sw.WriteLine("<form method = \"POST\">");
                                for (int j = 0; j < OszlopokKivalasztasa(Convert.ToInt32(tablaadatok[0])).Count; j++)
                                {
                                    string[] oszlopadatok = OszlopokKivalasztasa(Convert.ToInt32(tablaadatok[0]))[j].Split(';');
                                    if (oszlopadatok[2] == "INTEGER")
                                    {
                                        sw.WriteLine(oszlopadatok[1]);
                                        sw.WriteLine("\t<input type = \"number\" name = \"input_" + oszlopadatok[1] + "\"></br>");
                                    }
                                    if (oszlopadatok[2] == "VARCHAR" || oszlopadatok[2] == "TEXT")
                                    {
                                        sw.WriteLine(oszlopadatok[1]);
                                        sw.WriteLine("\t<input type = \"text\" name = \"input_" + oszlopadatok[1] + "\"></br>");
                                    }
                                    if (oszlopadatok[2] == "BOOLEAN")
                                    {
                                        sw.WriteLine(oszlopadatok[1]);
                                        sw.WriteLine("\t<select name = \"input_" + oszlopadatok[1] + "\">");
                                        sw.WriteLine("\t\t<option value = \"0\">Hamis</option>");
                                        sw.WriteLine("\t\t<option value = \"1\">Igaz</option>");
                                        sw.WriteLine("\t</select></br>");
                                    }
                                }
                                sw.WriteLine("\t<input type = \"hidden\" name = \"action\" value = \"cmd_insert_"+ tablaadatok[1] +"\">" +
                                     "\n\t</br><button type = \"submit\" > Felvétel </button>");
                                sw.WriteLine("</form>");
                            }
                        }
                        sw.WriteLine("<?php");
                        sw.WriteLine("class Osztaly{");
                        sw.WriteLine("\tvar $servername = \"localhost\";");
                        sw.WriteLine("\tvar $dbname = \""+ adatbazisNeveTextBox.Text +"\";");
                        sw.WriteLine("\tvar $username = \"root\";");
                        sw.WriteLine("\tvar $password = \"\";");
                        string kapcsolodasbontas = File.ReadAllText("kapcsolodasbontas.txt");
                        sw.WriteLine(kapcsolodasbontas);
                        for (int i = 0; i < TablakKivalasztasa().Count; i++)
                        {
                            string[] tablaAdatok = TablakKivalasztasa()[i].Split(';');
                            if (Convert.ToBoolean(tablaAdatok[2]))
                            {
                                string select = File.ReadAllText("select.txt");
                                select = select.Replace("[tabla]", tablaAdatok[1]);
                                select = select.Replace("[select]", "Select_" + tablaAdatok[1]);
                                string kiiratas = "echo ";
                                string delete;
                                List<string> oszlopok = new List<string>();
                                for (int j = 0; j < OszlopokKivalasztasa(Convert.ToInt32(tablaAdatok[0])).Count; j++)
                                {
                                    string[] oszlopadatok = OszlopokKivalasztasa(Convert.ToInt32(tablaAdatok[0]))[j].Split(';');
                                    oszlopok.Add(oszlopadatok[1]);
                                    if (OszlopMennyiseg(Convert.ToInt32(tablaAdatok[0])) > j + 1)
                                    {
                                        kiiratas += "$row[\"" + oszlopadatok[1] + "\"] . \", \" . ";

                                    }
                                    else
                                    {
                                        kiiratas += "$row[\"" + oszlopadatok[1] + "\"]";
                                        if (Convert.ToBoolean(tablaAdatok[4]))
                                        {
                                            delete = File.ReadAllText("deletebutton.txt");
                                            delete = delete.Replace("cmd_delete", "cmd_delete_" + tablaAdatok[1]);
                                            delete = delete.Replace("[ertek]", oszlopok[0]);
                                            select = select.Replace("[delete]", delete);
                                        }
                                        else
                                        {
                                            select = select.Replace("[delete]", "");
                                        }
                                    }
                                }
                                select = select.Replace("[kiiratas]", kiiratas);
                                sw.WriteLine(select);
                            }
                            List<string> oszlopNevek = new List<string>();
                            if (Convert.ToBoolean(tablaAdatok[3]))
                            {
                                string insert = File.ReadAllText("insert.txt");
                                insert = insert.Replace("[tablanev]", tablaAdatok[1]);
                                insert = insert.Replace("[insert]", "Insert_" + tablaAdatok[1]);
                                string oszlopok = "";
                                string inputok = "";
                                for (int j = 0; j < OszlopokKivalasztasa(Convert.ToInt32(tablaAdatok[0])).Count; j++)
                                {
                                    string[] oszlopadatok = OszlopokKivalasztasa(Convert.ToInt32(tablaAdatok[0]))[j].Split(';');
                                    oszlopNevek.Add(oszlopadatok[1]);
                                    if (OszlopMennyiseg(Convert.ToInt32(tablaAdatok[0])) > j+1)
                                    {
                                        oszlopok += oszlopadatok[1] + ", ";
                                        inputok += "\'\".$_POST[\"input_" + oszlopadatok[1] + "\"].\"\', ";
                                    }
                                    else
                                    {
                                        oszlopok += oszlopadatok[1];
                                        inputok += "\'\".$_POST[\"input_" + oszlopadatok[1] + "\"].\"\'";
                                    }
                                }
                                insert = insert.Replace("[oszlopnev]", oszlopok);
                                insert = insert.Replace("[input_mezok]", inputok);
                                sw.WriteLine(insert);
                            }
                            if (Convert.ToBoolean(tablaAdatok[4]))
                            {
                                string delete = File.ReadAllText("delete.txt");
                                delete = delete.Replace("delete", "Delete_" + tablaAdatok[1]);
                                delete = delete.Replace("[tablanev]", tablaAdatok[1]);
                                delete = delete.Replace("[oszlop]", oszlopNevek[0]);
                                sw.WriteLine(delete);
                            }

                        }
                        sw.WriteLine("}");
                        for (int i = 0; i < TablakKivalasztasa().Count; i++)
                        {
                            string[] tablaAdatok = TablakKivalasztasa()[i].Split(';');
                            if (Convert.ToBoolean(tablaAdatok[2]))
                            {
                                string select = File.ReadAllText("selectaction.txt");
                                select = select.Replace("[select]", "Select_" + tablaAdatok[1]);
                                select = select.Replace("[tabla]", tablaAdatok[1]);
                                sw.WriteLine(select);
                            }
                            if (Convert.ToBoolean(tablaAdatok[3]))
                            {
                                string insert = File.ReadAllText("insertaction.txt");
                                insert = insert.Replace("[insert]", "Insert_" + tablaAdatok[1]);
                                insert = insert.Replace("[cmd_insert]", "cmd_insert_" + tablaAdatok[1]);
                                sw.WriteLine(insert);
                            }
                            if (Convert.ToBoolean(tablaAdatok[4]))
                            {
                                string delete = File.ReadAllText("deleteaction.txt");
                                delete = delete.Replace("[delete]", "Delete_" + tablaAdatok[1]);
                                delete = delete.Replace("[cmd_delete]", "cmd_delete_" + tablaAdatok[1]);
                                sw.WriteLine(delete);
                            }
                        }
                        sw.WriteLine("?>");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Nem sikerült elmenteni a fájlt");
                }
            };
        }

        public List<string> TablakKivalasztasa()
        {
            List<string> tablak = new List<string>();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM tablak";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string tablanev = reader.GetString(1);
                    bool select = reader.GetBoolean(2);
                    bool insert = reader.GetBoolean(3);
                    bool delete = reader.GetBoolean(4);
                    bool update = reader.GetBoolean(5);
                    tablak.Add(id + ";" + tablanev + ";" + select + ";" + insert + ";" + delete + ";" + update);
                }
            }
            return tablak;
        }

        public List<string> OszlopokKivalasztasa(int index)
        {
            List<string> oszlopok = new List<string>();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM oszlopok WHERE tablaid = @id";
            cmd.Parameters.AddWithValue("@id", index);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string oszlopnev = reader.GetString(1);
                    string kiterjesztes = reader.GetString(2);
                    int hossz = reader.GetInt32(3);
                    bool autoinc = reader.GetBoolean(4);
                    bool prikey = reader.GetBoolean(5);
                    oszlopok.Add(id + ";" + oszlopnev + ";" + kiterjesztes + ";" + hossz + ";" + autoinc + ";" + prikey);
                }
            }
            return oszlopok;
        }
    }
}