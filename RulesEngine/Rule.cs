using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine
{
    public class Rule : IRule
    {
        public required string Id { get; set; }
        public required string Description { get; set; }
        public required bool Active { get; set; }
        public required string ErrorMessage { get; set; }
        public Dictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();
    }

    
}
