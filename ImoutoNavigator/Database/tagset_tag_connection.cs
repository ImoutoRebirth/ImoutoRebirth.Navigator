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
    
    public partial class tagset_tag_connection
    {
        public int id { get; set; }
        public int id_tagset { get; set; }
        public int id_tag { get; set; }
    
        public virtual tag tag { get; set; }
        public virtual tagset tagset { get; set; }
    }
}