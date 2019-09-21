using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DOST.DataAccess;

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

        public Cuenta(
            int id, string usuario, string password, string correo, int monedas,
            DateTime fechaCreacion, bool confirmada, string codigoValidacion
        ) {
            this.id = id;
            this.usuario = usuario;
            this.password = password;
            this.correo = correo;
            this.monedas = monedas;
            this.fechaCreacion = fechaCreacion;
            this.confirmada = confirmada;
            this.codigoValidacion = codigoValidacion;
        }

        public Cuenta(int id) {
            this.id = id;
            using (DostDatabase db = new DostDatabase()) {
                var cuentaDb = db.Cuenta.ToList().Find(account => account.idcuenta == id);
                if (cuentaDb != null) {
                    usuario = cuentaDb.usuario;
                    password = cuentaDb.password;
                    correo = cuentaDb.correo;
                    monedas = cuentaDb.monedas;
                    fechaCreacion = cuentaDb.fechaCreacion;
                    confirmada = cuentaDb.confirmada == 1;
                    codigoValidacion = cuentaDb.codigoValidacion;
                }
            }
        }
    }
}
