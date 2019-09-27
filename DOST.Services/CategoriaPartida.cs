using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
    public class CategoriaPartida {
        private int id;
        public int Id {
            get { return id; }
            set { id = value; }
        }
        private Partida partida;
        public Partida Partida {
            get { return partida; }
            set { partida = value; }
        }
        private string nombre;
        public string Nombre {
            get { return nombre; }
            set { nombre = value; }
        }

        public CategoriaPartida() {
        }

        public CategoriaPartida(int id, Partida partida, string nombre) {
            this.id = id;
            this.partida = partida;
            this.nombre = nombre;
        }
    }
}
