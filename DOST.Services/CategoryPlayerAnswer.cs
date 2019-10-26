﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DOST.Services {
    [DataContract]
    public class CategoryPlayerAnswer {
        private int id;
        [DataMember]
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Player player;
        [DataMember]
        public Player Player {
            get { return player; }
            set { player = value; }
        }
        private GameCategory gameCategory;
        [DataMember]
        public GameCategory GameCategory {
            get { return gameCategory; }
            set { gameCategory = value; }
        }
        private string answer;
        [DataMember]
        public string Answer {
            get { return answer; }
            set { answer = value; }
        }
        private int round;
        [DataMember]
        public int Round {
            get { return round; }
            set { round = value; }
        }
        private bool hasCorrectAnswer;
        [DataMember]
        public bool HasCorrectAnswer {
            get { return hasCorrectAnswer; }
            set { hasCorrectAnswer = value; }
        }

        public CategoryPlayerAnswer() {
        }
    }
}
