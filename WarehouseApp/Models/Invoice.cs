namespace WarehouseApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Invoice")]
    public partial class Invoice
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Invoice()
        {
            InvoiceProduct = new HashSet<InvoiceProduct>();
        }

        public int InvoiceId { get; set; }

        public DateTime Date { get; set; }

        public int DestinitonId { get; set; }

        public int RecipientId { get; set; }

        public virtual Destination Destination { get; set; }

        public virtual Recipient Recipient { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InvoiceProduct> InvoiceProduct { get; set; }
    }
}
