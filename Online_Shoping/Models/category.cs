//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Online_Shoping.Models
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    public partial class category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public category()
        {
            this.product_mst = new HashSet<product_mst>();
        }
    
        public int cat_id { get; set; }
        public string name { get; set; }
        public int adm_id { get; set; }
        public string image { get; set; }
        public HttpPostedFileBase File {  get; set; }


        public virtual Admin Admin { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<product_mst> product_mst { get; set; }
    }
}
