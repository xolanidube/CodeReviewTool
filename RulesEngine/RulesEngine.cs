using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Linq;
namespace RulesEngine
{
    public class RulesEngine
    {
        public readonly Dictionary<string, Dictionary<string, object>> _rules = new();
        public readonly Dictionary<string, IRuleEvaluator> _evaluators = new();
        private Contexts contexts = new Contexts();
        private string? path { get; set; }
        private XElement? xmlContent { get; set; }
        private RuleConfig ruleConfig { get; set; }


        public void Initialize(string path)
        {
            this.path = path;
            try
            {
                this.xmlContent = XElement.Load(path);
                contexts.LoadContexts(this.xmlContent);
            }
            catch (Exception ex) when (ex is IOException || ex is System.Xml.XmlException)
            {
                Console.WriteLine($"Could not load process file '{path}': {ex.Message}");
                throw;
            }
        }

        public void RegisterEvaluator(string ruleId, IRuleEvaluator evaluator)
        {
            _evaluators[ruleId] = evaluator;
        }

        public void AddRule(string ruleId, Dictionary<string, object> properties)
        {
            _rules[ruleId] = properties;
        }


        public RuleConfig LoadRuleConfig(string filePath)
        {
            try
            {
                var jsonString = File.ReadAllText(filePath);
                ruleConfig = JsonConvert.DeserializeObject<RuleConfig>(jsonString);
            }
            catch (Exception ex) when (ex is IOException || ex is JsonException)
            {
                Console.WriteLine($"Could not load rule configuration '{filePath}': {ex.Message}");
                throw;
            }

            // Manual validation (optional based on your needs)
            if (ruleConfig?.RuleGroups == null || !ruleConfig.RuleGroups.Any())
            {
                throw new JsonException("The configuration must contain at least one rule group.");
            }

            foreach (var ruleGroup in ruleConfig.RuleGroups.Values)
            {
                if (ruleGroup.Rules == null || !ruleGroup.Rules.Any())
                {
                    throw new JsonException("Each rule group must contain at least one rule.");
                }
            }

            return ruleConfig;
        }


        public bool Evaluate(string ruleId, object context, Dictionary<string, object>? additionalProperties)
        {
            if (_rules.TryGetValue(ruleId, out var properties) && _evaluators.TryGetValue(ruleId, out var evaluator))
            {

                return evaluator.Evaluate(ruleId, properties, context, additionalProperties);
            }
            throw new Exception($"Rule or evaluator for {ruleId} not found.");
        }

       public void AddRulesFromConfig()
        {
            // Iterate through each group in the configuration
            foreach (var groupEntry in ruleConfig.RuleGroups)
            {
                // Iterate through each rule within the group
                foreach (var ruleEntry in groupEntry.Value.Rules)
                {
                    // Extract rule ID and its properties
                    var ruleId = ruleEntry.Key;
                    var properties = ruleEntry.Value;

                    // Check if the rule is marked as active; since properties are in a dictionary,
                    // you need to safely cast the value to the expected type.
                    bool isActive = properties.TryGetValue("Active", out var activeValue) && Convert.ToBoolean(activeValue);

                    if (isActive)
                    {
                        AddRule(ruleId, properties);
                        // Assuming a GeneralRuleEvaluator that works for all types of rules
                        if (!_evaluators.ContainsKey(ruleId))
                        {
                            RegisterEvaluator(ruleId, new GeneralRuleEvaluator());
                        }
                    }
                }
            }
        }

        public void ValidateAll()
        {
            foreach (var ruleId in _rules.Keys)
            {
                var ruleProperties = _rules[ruleId];
                string ruleGroupKey = null;

                // Find the group this rule belongs to
                foreach (var groupEntry in ruleConfig.RuleGroups)
                {
                    if (groupEntry.Value.Rules.ContainsKey(ruleId))
                    {
                        ruleGroupKey = groupEntry.Key;
                        break;
                    }
                }

                // Check if the rule is marked as active
                if (ruleProperties.TryGetValue("Active", out var activeValue) && Convert.ToBoolean(activeValue))
                {
                    Dictionary<string, object> additionalProperties = new Dictionary<string, object>();
                    switch (ruleGroupKey)
                    {
                        case "Variables":
                            var sContexts = contexts.GetContexts<StageContext>();

                            if (ruleId == "VAR-001")
                            {
                                sContexts = sContexts.Where(s => s.Type.Equals("Data")).ToList();
                            }

                            // Example specific filter for rule VAR-002
                            if (ruleId == "VAR-002")
                            {
                                sContexts = sContexts.Where(s => !string.IsNullOrEmpty(s.Exposure)).ToList();
                            }

                            if (ruleId == "VAR-003")
                            {
                                sContexts = sContexts.Where(s => s.Type.Equals("Data") && s.Private != null).ToList();
                            }

                            if (ruleId == "VAR-004")
                            {
                                sContexts = sContexts.Where(s => s.Type.Equals("Data")).ToList();
                            }

                            if (ruleId == "VAR-005")
                            { 
                                sContexts = sContexts.Where(s => s.Type.Equals("Start") || s.Type.Equals("End")).ToList();
                            }

                            if(ruleId == "VAR-006")
                            {
                                var sBlocks = sContexts.Where(predicate: s => s.Type.Equals("Block")).ToList();
                                sContexts = sContexts.Where(s => s.Type.Equals("Data") && s.Private != null).ToList();
                                
                                additionalProperties.Add("Blocks", sBlocks);
                            }

                            

                            foreach (var context in sContexts)
                            {
                                bool result = Evaluate(ruleId, context, additionalProperties);
                                Console.WriteLine(result ? $"Validation passed for rule {ruleId}." : $"Validation failed for rule {ruleId}.");
                            }
                            break;
                        case "Pages":

                            var pContexts = contexts.GetContexts<PageContext>();

                            if (ruleId == "PAGE-001")
                            {
                                bool result = Evaluate(ruleId, pContexts.ToList()[0], additionalProperties);
                                Console.WriteLine(result ? $"Validation passed for rule {ruleId}." : $"Validation failed for rule {ruleId}.");
                            }
                            else
                            {
                                foreach (var context in pContexts)
                                {
                                    bool result = Evaluate(ruleId, context, additionalProperties);
                                    Console.WriteLine(result ? $"Validation passed for rule {ruleId}." : $"Validation failed for rule {ruleId}.");
                                }
                            }

                            
                            break;
                            // Handle other groups as needed
                    }
                }
            }
        }


    }
}
