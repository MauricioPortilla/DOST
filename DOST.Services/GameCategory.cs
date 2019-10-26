using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
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

        public GameCategory() {
        }

        public GameCategory(int id, Game game, string name) {
            this.id = id;
            this.game = game;
            this.name = name;
        }
    }
}
