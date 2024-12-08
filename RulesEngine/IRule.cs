using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngine
{
    public interface IRule
    {
        string Id { get; }
        string Description { get; }
        bool Active { get; }
        string ErrorMessage { get; }
    }

    public interface IRuleEvaluator
    {
        bool Evaluate(string ruleId, Dictionary<string, object> ruleProperties, object context, Dictionary<string, object>? additionalProperties);
    }

}