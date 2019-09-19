using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class Jugador {
        private int id;
        private Cuenta cuenta;
        public Cuenta Cuenta {
            get {
                return cuenta;
            }
        }
        private Partida partida;
        private int puntuacion;
        private bool anfitrion;
        public bool Anfitrion {
            get {
                return anfitrion;
            }
        }

        public Jugador(int id, Cuenta cuenta, Partida partida, int puntuacion, bool anfitrion) {
            this.id = id;
            this.cuenta = cuenta;
            this.partida = partida;
            this.puntuacion = puntuacion;
            this.anfitrion = anfitrion;
        }

    }
}
