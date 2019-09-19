using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DOST.Services {
    [DataContract]
    public class Cuenta {
        private int id;
        [DataMember]
        public int Id { get { return id; } set { id = value; } }
        private string usuario;
        [DataMember]
        public string Usuario { get { return usuario; } set { usuario = value; } }
        private string password; // cifrada con SHA1
        [DataMember]
        public string Password { get { return password; } set { password = value; } }
        private string correo;
        [DataMember]
        public string Correo { get { return correo; } set { correo = value; } }
        private int monedas;
        [DataMember]
        public int Monedas { get { return monedas; } set { monedas = value; } }
        private DateTime fechaCreacion;
        [DataMember]
        public DateTime FechaCreacion {
            get { return fechaCreacion; }
            set { fechaCreacion = value; }
        }
        private bool confirmada;
        [DataMember]
        public bool Confirmada {
            get { return confirmada; }
            set { confirmada = value; }
        }
        private string codigoValidacion;
        [DataMember]
        public string CodigoValidacion {
            get { return codigoValidacion; }
            set { codigoValidacion = value; }
        }

        public Cuenta() {
        }

        public Cuenta(int id) {
            this.id = id;
            Database.ExecuteStoreQuery(
                "SELECT * FROM cuenta WHERE idcuenta = @idcuenta",
                new Dictionary<string, object>() {
                    { "@idcuenta", id }
                }, (results) => {
                    var row = results[0];
                    usuario = row["usuario"].ToString();
                    password = row["password"].ToString();
                    correo = row["correo"].ToString();
                    monedas = (int) row["monedas"];
                    fechaCreacion = DateTime.Parse(row["fechaCreacion"].ToString());
                    confirmada = (int) row["confirmado"] == 1;
                    codigoValidacion = row["codigoValidacion"].ToString();
                }
            );
        }
    }
}
