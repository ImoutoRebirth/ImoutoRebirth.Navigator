//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImoutoNavigator.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class tag_type
    {
        public tag_type()
        {
            this.tags = new HashSet<tag>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
    
        public virtual ICollection<tag> tags { get; set; }
    }
}