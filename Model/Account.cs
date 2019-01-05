using System;
using System.ComponentModel.DataAnnotations;

namespace hoppa.Service.Model
{
    public class Account
    {
        [Key]
        public string Guid { get; set; } = new Guid().ToString();
        [Required]
        public string Type { get; set; } = string.Empty;
        [Required]
        public string IBAN { get; set; } = string.Empty;
        [Required]
        public string AccessRights { get; set; } = string.Empty;

        public string Description { get; set; }
        public double Balance { get; set; }
    }

    public class OtherAccount : Account
    {
        [Required]
        public string OwnerName { get; set; } = string.Empty;
    }
}