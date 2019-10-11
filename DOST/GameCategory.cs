using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class GameCategory {
        private int id;
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Game game;
        public Game Game {
            get { return game; }
            set { game = value; }
        }
        private string name;
        public string Name {
            get { return name; }
            set { name = value; }
        }

        public GameCategory(int id, Game game, string name) {
            this.id = id;
            this.game = game;
            this.name = name;
        }
    }
}
