//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BankingSystem
{
    using System;
    using System.Collections.Generic;
    
    public partial class deposite_type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public deposite_type()
        {
            this.bank_deposit = new HashSet<bank_deposit>();
        }
    
        public int id { get; set; }
        public int Term { get; set; }
        public int Interest_rate { get; set; }
        public bool Capitalization { get; set; }
        public bool Early_closure { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<bank_deposit> bank_deposit { get; set; }
    }
}
