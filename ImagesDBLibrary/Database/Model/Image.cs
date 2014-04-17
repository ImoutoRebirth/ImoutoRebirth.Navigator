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
    
    public partial class Image
    {
        public Image()
        {
            this.TagSets = new HashSet<TagSet>();
            this.Sources = new HashSet<Source>();
        }
    
        public int Id { get; set; }
        public string Md5 { get; set; }
        public long Size { get; set; }
        public string Path { get; set; }
    
        public virtual ICollection<TagSet> TagSets { get; set; }
        public virtual ICollection<Source> Sources { get; set; }
    }
}
