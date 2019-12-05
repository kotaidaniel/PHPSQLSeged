using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHPSQLSeged
{
    class Oszlopok
    {
        int id;
        string oszlopNeve;
        string kiterjesztes;
        bool autoincrement;
        bool primaryKey;
        int tablaid;

        public Oszlopok(int id, string oszlopNeve, string kiterjesztes, bool autoincrement, bool primaryKey, int tablaid)
        {
            this.Id = id;
            this.OszlopNeve = oszlopNeve;
            this.Kiterjesztes = kiterjesztes;
            this.Autoincrement = autoincrement;
            this.PrimaryKey = primaryKey;
            this.Tablaid = tablaid;
        }

        public int Id { get => id; set => id = value; }
        public string OszlopNeve { get => oszlopNeve; set => oszlopNeve = value; }
        public string Kiterjesztes { get => kiterjesztes; set => kiterjesztes = value; }
        public bool Autoincrement { get => autoincrement; set => autoincrement = value; }
        public bool PrimaryKey { get => primaryKey; set => primaryKey = value; }
        public int Tablaid { get => tablaid; set => tablaid = value; }

        public override string ToString()
        {
            return OszlopNeve;
        }
    }
}
