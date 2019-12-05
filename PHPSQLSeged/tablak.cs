using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHPSQLSeged
{
    class Tablak
    {
        int id;
        string tablanev;

        public Tablak(int id, string tablanev)
        {
            this.Id = id;
            this.Tablanev = tablanev;
        }

        public int Id { get => id; set => id = value; }
        public string Tablanev { get => tablanev; set => tablanev = value; }

        public override string ToString()
        {
            return tablanev;
        }
    }
}
