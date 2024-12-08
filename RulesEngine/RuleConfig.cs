namespace RulesEngine
{
    public class RuleConfig
    {
        public Dictionary<string, RuleGroup> RuleGroups { get; set; }
    }

    public class RuleGroup
    {
        // This directly maps each rule to its properties
        public Dictionary<string, Dictionary<string, object>> Rules { get; set; }
    }






}
