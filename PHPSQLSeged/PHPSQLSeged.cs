using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PHPSQLSeged
{
    public partial class PHPSQLSeged : Form
    {

        public PHPSQLSeged()
        {
            InitializeComponent();
            sqlPanel.Visible = false;
              
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

        private void kilepesButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
