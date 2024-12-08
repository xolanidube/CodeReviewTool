using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RulesEngine
{
    internal class Variables
    {
    }

    public class VariableRule : IRule
    {
        public required string Id { get; set; }
        public required string Description { get; set; }
        public required bool Active { get; set; }
        public required string ErrorMessage { get; set; }
        public List<string>? Characters { get; set; }
        public List<string>? Prefixes { get; set; }
        public List<string>? Suffixes { get; set; }
        public List<string>? MultiwordDelimited { get; set; }
        public List<string>? HungarianNotation { get; set; }

        public List<string>? StatisticPrefixes { get; set; }

        public List<string>? SessionPrefixes { get; set; }

        public List<string>? EnvironmentPrefixes { get; set; }
    }

    public class VAR001 : IRuleEvaluator
    {
        public bool Evaluate(IRule rule, object context)
        {
            if (rule is VariableRule varRule && context is string variableName)
            {

                // Step 1: Validate and extract prefix
                var prefix = varRule.Prefixes?.FirstOrDefault(variableName.StartsWith);
                if (varRule.Prefixes != null && prefix == null) return false; // Prefixes defined but none match
                var strippedName = prefix != null ? variableName.Substring(prefix.Length) : variableName;

                // Step 2: Validate and extract suffixes
                var suffix = varRule.Suffixes?.FirstOrDefault(strippedName.EndsWith);
                if (varRule.Suffixes != null && suffix == null) return false; // Suffixes defined but none match
                strippedName = suffix != null ? strippedName[..^suffix.Length] : strippedName;

                // Step 3: Strip defined characters to isolate core name for multiword and Hungarian validation
                var coreName = StripDefinedCharacters(varRule, strippedName);

                // Step 4: Validate Hungarian Notation
                if (varRule.HungarianNotation != null && !varRule.HungarianNotation.Any(coreName.StartsWith)) return false;

                // Step 5: Validate multiword delimited
                if (varRule.MultiwordDelimited != null && !IsValidNamingConvention(varRule, coreName)) return false;

                return true;
            }
            return false;
        }




        private string StripDefinedCharacters(VariableRule rule, string name)
        {
            if (rule.Characters == null) return name;
            foreach (var character in rule.Characters.Where(c => !string.IsNullOrEmpty(c)))
            {
                name = name.Replace(character, "");
            }
            return name;
        }

        private bool IsValidNamingConvention(VariableRule rule, string name)
        {
            // Implement multiword delimitation validation logic here
            // This is simplified and should be expanded based on specific rule requirements
            return rule.MultiwordDelimited.Contains("camelcase") && char.IsLower(name[0]) ||
                   rule.MultiwordDelimited.Contains("pascalcase") && char.IsUpper(name[0]) ||
                   rule.MultiwordDelimited.Contains("snakecase") && name.Contains('_');
        }

    }

    public class VAR002 : IRuleEvaluator
    {
        public bool Evaluate(IRule rule, object context)
        {
            if (!(rule is VariableRule varRule) || !(context is StageContext stageContext)) return false;

            var validPrefixes = stageContext.Exposure switch
            {
                "Statistic" => varRule.StatisticPrefixes,
                "Session" => varRule.SessionPrefixes,
                "Environment" => varRule.EnvironmentPrefixes,
                _ => null
            };

            if (validPrefixes == null || !validPrefixes.Any(prefix => stageContext.Name.StartsWith(prefix)))
            {
                Console.WriteLine(varRule.ErrorMessage.Replace("{NAMEOFVAR}", stageContext.Name));
                return false;
            }

            return true;
        }

        List<StageContext> ExtractStageContexts(XElement xmlContent)
        {
            var stages = xmlContent.Descendants("stage")
                .Select(stage => new StageContext
                {
                    Name = (string)stage.Attribute("name"),
                    Type = (string)stage.Attribute("type"),
                    Exposure = (string)stage.Element("exposure") // Assuming 'exposure' is a direct child of 'stage'
                })
                .ToList();

            return stages;
        }
    }


    public class VAR003 : IRuleEvaluator
    {
        public bool Evaluate(IRule rule, object context)
        {
            throw new NotImplementedException();
        }
    }

    public class VAR004 : IRuleEvaluator
    {
        public bool Evaluate(IRule rule, object context)
        {
            throw new NotImplementedException();
        }
    }

    public class VAR005 : IRuleEvaluator
    {
        public bool Evaluate(IRule rule, object context)
        {
            throw new NotImplementedException();
        }
    }

    public class StageContext
    {
        public string stageid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Exposure { get; set; }
    }
    

}
