using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
    [DataContract]
    public class Jugador {
        private int id;
        [DataMember]
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Cuenta cuenta;
        [DataMember]
        public Cuenta Cuenta {
            get { return cuenta; }
            set { cuenta = value; }
        }
        private Partida partida;
        [DataMember]
        public Partida Partida {
            get { return partida; }
            set { partida = value; }
        }
        private int puntuacion;
        [DataMember]
        public int Puntuacion {
            get { return puntuacion; }
            set { puntuacion = value; }
        }
        private bool anfitrion;
        [DataMember]
        public bool Anfitrion {
            get { return anfitrion; }
            set { anfitrion = value; }
        }

        public Jugador(int id, Cuenta cuenta, Partida partida, int puntuacion, bool anfitrion) {
            this.id = id;
            this.cuenta = cuenta;
            this.partida = partida;
            this.puntuacion = puntuacion;
            this.anfitrion = anfitrion;
        }

        public Jugador() {
        }
    }
}
