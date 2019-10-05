using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace DOST.Services {
    public class UserScore {
        public int Posicion;
        public string Usuario;
        public int Puntuacion;

        public UserScore() {
        }

        public UserScore(int posicion, string usuario, int puntuacion) {
            Posicion = posicion;
            Usuario = usuario;
            Puntuacion = puntuacion;
        }
    }
}
