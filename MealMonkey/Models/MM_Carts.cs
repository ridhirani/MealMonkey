//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MealMonkey.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Xunit;

    public partial class MM_Carts
    {
        public int CartId { get; set; }
        public Nullable<int> ProductId { get; set; }
        [Range(1, 10, ErrorMessage = "You can only enter 1 to 10")]
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> UserId { get; set; }
    }
}