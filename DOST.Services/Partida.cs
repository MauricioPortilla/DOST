using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
    [DataContract]
    public class Partida {
        private int id;
        [DataMember]
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private int ronda;
        [DataMember]
        public int Ronda {
            get { return ronda; }
            set { ronda = value; }
        }
        private DateTime fecha;
        [DataMember]
        public DateTime Fecha {
            get { return fecha; }
            set { fecha = value; }
        }
        private List<Jugador> jugadores;
        [DataMember]
        public List<Jugador> Jugadores {
            get { return jugadores; }
            set { jugadores = value; }
        }

        public Partida() {
        }

        public Partida(int id, int ronda, DateTime fecha) {
            this.id = id;
            this.ronda = ronda;
            this.fecha = fecha;
            jugadores = new List<Jugador>();
        }
    }
}
