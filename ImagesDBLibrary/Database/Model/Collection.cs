//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImagesDBLibrary.Database.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Collection
    {
        public Collection()
        {
            this.Sources = new HashSet<Source>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Source> Sources { get; set; }
    }
}
