using System.Collections.Generic;

namespace DOST {

    /// <summary>
    /// Represents a category in the game.
    /// </summary>
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

        /// <summary>
        /// Creates a GameCategory instance by data given.
        /// </summary>
        /// <param name="id">GameCategory identifier</param>
        /// <param name="game">Game where category belongs to</param>
        /// <param name="name">GameCategory name</param>
        public GameCategory(int id, Game game, string name) {
            this.id = id;
            this.game = game;
            this.name = name;
        }
    }
}
