using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace hoppa.Service.Model
{
    public class Rule
    {
        [Key]
        public string Guid { get; set; }
        [Required]
        public string Name { get; set; }

        public Condition Condition { get; set; }

        public List<Action> Actions { get; set; }
    }
}