using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;

using System.Threading.Tasks;

using hoppa.Service.Interfaces;
using hoppa.Service.Model;

namespace hoppa.Service.Intergrations.splitwise
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
