//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DOST.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class CategoriaPartida
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CategoriaPartida()
        {
            this.RespuestaCategoriaJugador = new HashSet<RespuestaCategoriaJugador>();
        }
    
        public int idcategoria { get; set; }
        public int idpartida { get; set; }
        public string nombre { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RespuestaCategoriaJugador> RespuestaCategoriaJugador { get; set; }
    }
}
