using System.Collections.Generic;

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
        private List<CategoryPlayerAnswer> categoryPlayerAnswers = new List<CategoryPlayerAnswer>();
        public List<CategoryPlayerAnswer> CategoryPlayerAnswer {
            get { return categoryPlayerAnswers; }
            set { categoryPlayerAnswers = value; }
        }

        public GameCategory(int id, Game game, string name) {
            this.id = id;
            this.game = game;
            this.name = name;
        }
    }
}
