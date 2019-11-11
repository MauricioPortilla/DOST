using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DOST.Services {
    /// <summary>
    /// Represents a game category in service.
    /// </summary>
    [DataContract]
    public class GameCategory {
        private int id;
        [DataMember]
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Game game;
        [DataMember]
        public Game Game {
            get { return game; }
            set { game = value; }
        }
        private string name;
        [DataMember]
        public string Name {
            get { return name; }
            set { name = value; }
        }
        private List<CategoryPlayerAnswer> categoryPlayerAnswers = new List<CategoryPlayerAnswer>();
        [DataMember]
        public List<CategoryPlayerAnswer> CategoryPlayerAnswer {
            get { return categoryPlayerAnswers; }
            set { categoryPlayerAnswers = value; }
        }

        /// <summary>
        /// Creates and initializes a new instance.
        /// </summary>
        public GameCategory() {
        }

        /// <summary>
        /// Creates and initializes a new instance with data given.
        /// </summary>
        /// <param name="id">GameCategory identifier</param>
        /// <param name="game">Game where it belongs to</param>
        /// <param name="name">GameCategory name</param>
        public GameCategory(int id, Game game, string name) {
            this.id = id;
            this.game = game;
            this.name = name;
        }
    }
}
