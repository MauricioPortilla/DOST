using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace DOST.Services {
    public class UserScore {
        public int Ranking;
        public string Username;
        public int Score;

        public UserScore() {
        }

        public UserScore(int ranking, string username, int score) {
            Ranking = ranking;
            Username = username;
            Score = score;
        }
    }
}
