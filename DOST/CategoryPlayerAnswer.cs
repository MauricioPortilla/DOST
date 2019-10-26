using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class CategoryPlayerAnswer {
        private int id;
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Player player;
        public Player Player {
            get { return player; }
            set { player = value; }
        }
        private GameCategory gameCategory;
        public GameCategory GameCategory {
            get { return gameCategory; }
            set { gameCategory = value; }
        }
        private string answer;
        public string Answer {
            get { return answer; }
            set { answer = value; }
        }
        private int round;
        public int Round {
            get { return round; }
            set { round = value; }
        }
        private bool hasCorrectAnswer;
        public bool HasCorrectAnswer {
            get { return hasCorrectAnswer; }
            set { hasCorrectAnswer = value; }
        }

        public CategoryPlayerAnswer(int id, Player player, GameCategory gameCategory, string answer, int round) {
            this.id = id;
            this.player = player;
            this.gameCategory = gameCategory;
            this.answer = answer;
            this.round = round;
        }
    }
}
