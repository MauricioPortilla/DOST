using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class CategoriaPartida {
        private int id;
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Partida game;
        public Partida Game {
            get { return game; }
            set { game = value; }
        }
        private string name;
        public string Name {
            get { return name; }
            set { name = value; }
        }

        public CategoriaPartida(int id, Partida game, string name) {
            this.id = id;
            this.game = game;
            this.name = name;
        }
    }
}
