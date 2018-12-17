using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace hoppa.Service.Model
{
    public class Connection
    {
        [Key]
        public string Guid { get; set; } = new Guid().ToString();
        
        [Required]
        public string Type { get; set; }
        public string DisplayName { get; set; }
        public int ExternalId { get; set; } = 0;
        public string UserName { get; set; }
        public IDictionary<string, object> Parameters { get; set; }  
    }

    public class Registration
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Code { get; set; }
    }
}