//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IMSDBLayer.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public System.Guid Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<decimal> AuthorisedHours { get; set; }
        public Nullable<decimal> AuthorisedCosts { get; set; }
        public string IdentityId { get; set; }
        public Nullable<System.Guid> DistrictId { get; set; }
    }
}
