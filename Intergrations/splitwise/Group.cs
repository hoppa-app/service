using System;
using System.ComponentModel.DataAnnotations;

namespace hoppa.Service.Intergrations.Splitwise
{
    public class Group
    {
        [Key]
        public string Guid { get; set; } = new Guid().ToString();
        public Int32 GroupId { get; set; }

        public string Description { get; set; }

        public double Balance { get; set; }

    }
}
