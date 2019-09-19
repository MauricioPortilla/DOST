using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOST {
    public class Partida : INotifyPropertyChanged {
        private int id;
        public int Id {
            get {
                return id;
            }
        }
        private int ronda;
        private DateTime fecha;
        private List<Jugador> jugadores;
        public List<Jugador> Jugadores {
            get {
                return jugadores;
            } set {
                jugadores = value;
                var anfitrion = jugadores.Find(x => x.Anfitrion == true);
                if (anfitrion != null) {
                    Nombre = "Partida de " + anfitrion.Cuenta.Usuario;
                }
                if (numeroJugadores != jugadores.Count) {
                    NumeroJugadores = jugadores.Count.ToString();
                }
            }
        }
        public string Nombre { get; set; }
        private int numeroJugadores;
        public string NumeroJugadores {
            get {
                return numeroJugadores + "/4";
            } set {
                numeroJugadores = int.Parse(value);
                NotifyPropertyChanged("NumeroJugadores");
            }
        }

        public Partida(int id, int ronda, DateTime fecha, List<Jugador> jugadores) {
            this.id = id;
            this.ronda = ronda;
            this.fecha = fecha;
            Jugadores = jugadores;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string obj) {
            if (PropertyChanged != null) {
                this.PropertyChanged(this, new PropertyChangedEventArgs(obj));
            }
        }
    }
}
