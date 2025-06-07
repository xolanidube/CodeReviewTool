using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RulesEngine
{
    public class GeneralRuleEvaluator : IRuleEvaluator
    {
        public bool Evaluate(string ruleId, Dictionary<string, object> ruleProperties, object context, Dictionary<string, object>? additionalprops)
        {

            // Handle StageContext
            if (context is StageContext stageContext)
            {
                return EvaluateBasedOnContextType(ruleId, ruleProperties, stageContext, additionalprops);
            }
            // Handle PageContext
            else if (context is PageContext pageContext)
            {
                return EvaluateBasedOnContextType(ruleId, ruleProperties, pageContext, additionalprops);
            }
            // Handle other context types as necessary
            else
            {
                // This block can be used to log or handle unsupported context types appropriately
                throw new NotImplementedException("Context type not supported.");
            }
        }

        private bool EvaluateBasedOnContextType(string ruleId, Dictionary<string, object> ruleProperties, StageContext stageContext, Dictionary<string, object>? additionalprops)
        {
            // Example: Direct the evaluation to a specific method based on ruleId
            // You can expand this switch-case to include different rules and their respective evaluation methods
            switch (ruleId)
            {
                case "VAR-001":
                    return EvaluateVar001(ruleProperties, stageContext);
                case "VAR-002":
                    return EvaluateVar002(ruleProperties, stageContext);
                case "VAR-003":
                    return EvaluateVar003(ruleProperties, stageContext);
                case "VAR-004":
                    return EvaluateVar004(ruleProperties, stageContext);
                case "VAR-005":
                    return EvaluateVar005(ruleProperties, stageContext);
                case "VAR-006":
                    return EvaluateVar006(ruleProperties, stageContext, additionalprops);
                case "VAR-007":
                    return EvaluateVar007(ruleProperties, stageContext);

                case "SEC-001":
                    return EvaluateSec001(ruleProperties, stageContext);
                case "SEC-002":
                    return EvaluateSec002(ruleProperties, stageContext);
                case "SEC-003":
                    return EvaluateSec003(ruleProperties, stageContext);
                case "ENV-001":
                    return EvaluateEnv001(ruleProperties, stageContext);
                case "ENV-002":
                    return EvaluateEnv002(ruleProperties, stageContext);
                case "ENV-001":
                    return EvaluateEnv001(ruleProperties, stageContext);
                case "LOG-001":
                    return EvaluateLog001(ruleProperties, stageContext, additionalprops);
                case "LOG-002":
                    return EvaluateLog002(ruleProperties, stageContext);
                case "LOG-003":
                    return EvaluateLog003(ruleProperties, stageContext);


                case "STAGE-001":
                    return EvaluateStage001(ruleProperties, stageContext);
                case "STAGE-002":
                    return EvaluateStage002(ruleProperties, stageContext, additionalprops);

                // Add cases for other VAR-XXX as needed
                default:
                    throw new NotImplementedException($"Evaluator for {ruleId} is not implemented.");
            }
        }


        private bool EvaluateBasedOnContextType(string ruleId, Dictionary<string, object> ruleProperties, PageContext pageContext, Dictionary<string, object>? additionalprops)
        {
            // Similarly, evaluation logic specific to PageContext can be implemented here
            // This can be structured similarly to the StageContext evaluation, with methods tailored to handle PageContext specifics
            switch (ruleId)
            {
                // Example rule evaluation specific to PageContext
                case "PAGE-001":
                    return EvaluatePage001(ruleProperties, pageContext);
                case "PAGE-002":
                    return EvaluatePage002(ruleProperties, pageContext);
                case "PAGE-003":
                    return EvaluatePage003(ruleProperties, pageContext);
                // Add more cases as necessary
                default:
                    throw new NotImplementedException($"Evaluation for rule {ruleId} is not implemented for PageContext.");
            }

            return false; // Placeholder return statement
        }

        private bool EvaluateVar001(Dictionary<string, object> properties, StageContext context)
        {
            // Initialize variables to hold the specific details for the error message
            string expectedPrefixes = String.Join(", ", JsonConvert.DeserializeObject<List<string>>(properties["Prefixes"].ToString()));
            string expectedCasing = String.Join(", ", JsonConvert.DeserializeObject<List<string>>(properties["MultiwordDelimited"].ToString()));
            string expectedHungarianNotation = String.Join(", ", JsonConvert.DeserializeObject<List<string>>(properties["HungarianNotation"].ToString()));
            List<string> failureReasons = new List<string>();

            var variableName = context.Name; // Assuming this is the variable name
            var errorMessageTemplate = properties["Error Message"].ToString();

            // Assuming these properties have been deserialized correctly from JSON
            var prefixes = JsonConvert.DeserializeObject<List<string>>(properties["Prefixes"].ToString());
            var suffixes = JsonConvert.DeserializeObject<List<string>>(properties["Suffixes"].ToString());
            var characters = JsonConvert.DeserializeObject<List<string>>(properties["Characters"].ToString());
            var hungarianNotation = JsonConvert.DeserializeObject<List<string>>(properties["HungarianNotation"].ToString());
            var multiwordDelimited = JsonConvert.DeserializeObject<List<string>>(properties["MultiwordDelimited"].ToString());


            // Step 1: Validate and extract prefix
            var prefix = prefixes?.FirstOrDefault(variableName.StartsWith);

            if (prefixes != null && prefix == null && prefixes.Count != 0) failureReasons.Add("missing a valid prefix");
            var strippedName = prefix != null ? variableName.Substring(prefix.Length) : variableName;

            // Step 2: Validate and extract suffixes
            var suffix = suffixes?.FirstOrDefault(strippedName.EndsWith);
            if (suffixes != null && suffix == null && suffixes.Count != 0) failureReasons.Add("missing a valid suffix");
            strippedName = suffix != null ? strippedName[..^suffix.Length] : strippedName;

            // Step 3: Strip defined characters to isolate core name for multiword and Hungarian validation
            var coreName = StripDefinedCharacters(characters, strippedName);

            // Step 4: Validate Hungarian Notation
            var hungarianNot = hungarianNotation?.FirstOrDefault(coreName.StartsWith);
            coreName = hungarianNotation != null && hungarianNot != null ? coreName.Substring(hungarianNot.Length) : coreName;
            if (hungarianNotation != null && hungarianNot == null && hungarianNotation.Count != 0) failureReasons.Add("lacking or having incorrect Hungarian notation");
            

            // Step 5: Validate multiword delimited
            if (multiwordDelimited != null && !IsValidNamingConvention(multiwordDelimited, coreName == null ? strippedName : coreName)) failureReasons.Add("not adhering to specified casing conventions");

            // Construct the error message based on the validations
            string errorMessage = errorMessageTemplate
                .Replace("{NAMEOFVAR}", variableName)
                .Replace("{EXPECTEDPREFIXES}", expectedPrefixes)
                .Replace("{CASESTYLE}", expectedCasing)
                .Replace("{HUNGARIANNOTATION}", expectedHungarianNotation);

            // If there are any failure reasons, adjust the error message and return false
            if (failureReasons.Any())
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }

        private bool EvaluateVar002(Dictionary<string, object> properties, StageContext context)
        {

            var Statistic = JsonConvert.DeserializeObject<List<string>>(properties["Statistic Prefixes"].ToString());
            var Session = JsonConvert.DeserializeObject<List<string>>(properties["Session Prefixes"].ToString());
            var Environment = JsonConvert.DeserializeObject<List<string>>(properties["Environment Prefixes"].ToString());
            var errorMessageTemplate = properties["Error Message"] as string;

            var validPrefixes = context.Exposure switch
            {
                "Statistic" => Statistic,
                "Session" => Session,
                "Environment" => Environment,
                _ => null
            };

            string errorMessage = errorMessageTemplate
                .Replace("{NAMEOFVAR}", context.Name)
                .Replace("{EXPOSURE}", context.Exposure)
                .Replace("{EXPOSURETYPE}", context.Exposure)
                .Replace("{EXPECTEDPREFIXES}", string.Join(", ", validPrefixes));

            if (validPrefixes == null || !validPrefixes.Any(prefix => context.Name.StartsWith(prefix)))
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }

        private bool EvaluateVar003(Dictionary<string, object> properties, StageContext context)
        {
            // Initialize variables to hold the specific details for the error message
            string expectedPrefixes = String.Join(", ", JsonConvert.DeserializeObject<List<string>>(properties["Prefixes"].ToString()));
            List<string> failureReasons = new List<string>();

            var variableName = context.Name; // Assuming this is the variable name
            var errorMessageTemplate = properties["Error Message"].ToString();

            // Assuming these properties have been deserialized correctly from JSON
            var prefixes = JsonConvert.DeserializeObject<List<string>>(properties["Prefixes"].ToString());

            //Step 1: Check if its not private and has a valid prefix
            var prefix = prefixes?.FirstOrDefault(variableName.StartsWith);
            if (context.Private == false && prefix != null)
            {
                //failureReasons.Add("Visibility is global and has correct prefix, please ensure that the visibility is global.");
            }
            else if (context.Private == false && prefix == null)
            { 
                failureReasons.Add("Visibility is global and missing a valid prefix, ensure that the variable has the correct prefix.");
            }else if(context.Private == true && prefix != null)
            {
                failureReasons.Add("Visibility is private and has a correct prefix, please ensure that the visibility is global.");
            }
            // Construct the error message based on the validations
            string errorMessage = errorMessageTemplate
                .Replace("{NAMEOFVAR}", variableName)
                .Replace("{EXPECTEDPREFIXES}", expectedPrefixes);
                

            // If there are any failure reasons, adjust the error message and return false
            if (failureReasons.Any())
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;


        }

        private bool EvaluateVar004(Dictionary<string, object> properties, StageContext context)
        {
            int Length = Convert.ToInt32(properties["Length"]);
            var variableName = context.Name; // Assuming this is the variable name

            var errorMessageTemplate = properties["Error Message"].ToString();

            string errorMessage = errorMessageTemplate
                .Replace("{NAMEOFVAR}", variableName)
                .Replace("{LENGTH}", Length.ToString());


            if (variableName.Length > Length)
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            // Implement the logic for VAR-004
            return true;
        }

        public bool EvaluateVar005(Dictionary<string, object> properties, StageContext context)
        {
            // Deserialize properties from JSON
            var startPrefixes = JsonConvert.DeserializeObject<List<string>>(properties["Start Stage Allowed Prefixes"].ToString());
            var endPrefixes = JsonConvert.DeserializeObject<List<string>>(properties["End Stage Allowed Prefixes"].ToString());

            // Initialize variables to hold the specific details for constructing the error message
            string EligibleStartPrefixes = String.Join(", ", startPrefixes);
            string EligibleEndPrefixes = String.Join(", ", endPrefixes);

            var errorMessageTemplate = properties["Error Message"].ToString();

            // Determine if context is a Start or End stage and get the corresponding inputs/outputs
            IEnumerable<string> variableNames = context.Type == "Start" ? context.Inputs : context.Outputs;
            List<string> incorrectVariables = new List<string>();
            string expectedPrefixes = context.Type == "Start" ? EligibleStartPrefixes : EligibleEndPrefixes;

            foreach (var variableName in variableNames)
            {
                // Determine if variable starts with any of the eligible prefixes
                bool hasCorrectPrefix = (context.Type == "Start" ? startPrefixes : endPrefixes)
                                        .Any(prefix => variableName.StartsWith(prefix));

                // If variable doesn't start with a correct prefix, add to the incorrectVariables list
                if (!hasCorrectPrefix)
                {
                    incorrectVariables.Add(variableName);
                }
            }

            // Check if there were any incorrect variables found
            if (incorrectVariables.Any())
            {
                // Construct the error message for each incorrect variable
                foreach (var incorrectVariable in incorrectVariables)
                {
                    // Assuming we don't have a way to determine the incorrect prefix used, if any.
                    string errorMessage = errorMessageTemplate
                        .Replace("{NAMEOFVAR}", incorrectVariable)
                        .Replace("{PREFIX}", "an incorrect prefix")
                        .Replace("{EXPECTEDSTAGES}", context.Type == "Start" ? "start" : "end")
                        .Replace("{ELIGIBLEPREFIXES}", expectedPrefixes);

                    // Output or log the error message
                    Console.WriteLine(errorMessage);
                }
                return false; // Indicate failure due to incorrect prefix usage
            }

            return true; // All variables passed the prefix check
        }

        public bool EvaluateVar006(Dictionary<string, object> properties, StageContext context, Dictionary<string, object>? additionalProps)
        {
            if (additionalProps == null || !additionalProps.TryGetValue("Blocks", out var blocksObj) || !(blocksObj is List<StageContext> Blocks))
            {
                throw new ArgumentException("Blocks not provided or invalid in additionalProps.");
            }

            // Assuming ParseBlockProperties is implemented elsewhere to extract Name and Color.
            var blockColors = new Dictionary<string, (string Name, string Color)>
            {
                { "Environment", ParseBlockProperties(properties["Environment Variables Color"]) },
                { "Global", ParseBlockProperties(properties["Global Variables Color"]) },
                { "Local", ParseBlockProperties(properties["Local Variables Color"]) },
                { "Collection", ParseBlockProperties(properties["Collections Color"]) },
                { "Global Collections", ParseBlockProperties(properties["Global Collections Color"]) },
                { "Process Settings", ParseBlockProperties(properties["Process Settings Color"]) },
            };
            bool isRulePassed = false;
            string failedBlockName = string.Empty;
            string failedBlockColor = string.Empty;
            // Filter blocks that have names matching the specified names in blockColors.
            var matchingBlocks = Blocks
                .Where(b => blockColors.Values.Select(bc => bc.Name).Contains(b.Name) && b.PageName == context.PageName)
                .ToList();

            foreach (var block in matchingBlocks)
            {
                if (IsContextWithinBlock(context, block))
                {
                    var blockColorNamePair = blockColors.FirstOrDefault(bc => bc.Value.Color == block.Font.Color);
                    if (!blockColorNamePair.Equals(default(KeyValuePair<string, (string Name, string Color)>)))
                    {
                        // Block and color conditions met, rule passed for this context.
                        isRulePassed = true;
                        break;
                    }
                    else
                    {
                        // Store the failed block name and color for the error message.
                        failedBlockName = block.Name;
                        failedBlockColor = block.Font.Color;

                    }
                }
            }

            // Rule failed, construct and display the error message.
            if (!isRulePassed)
            {
                // Rule failed, construct and display the error message.
                string errorMessage = properties["Error Message"].ToString()
                    .Replace("{NAMEOFVAR}", context.Name)
                    .Replace("{BLOCKNAME}", failedBlockName)
                    .Replace("{BLOCKCOLOR}", failedBlockColor)
                    .Replace("{PAGENAME}", context.PageName);

                Console.WriteLine(errorMessage);
                return false;
            }


            return true;
        }

        private bool EvaluateVar007(Dictionary<string, object> properties, StageContext context)
        {
            var variableName = context.Name;
            var errorMessageTemplate = properties["Error Message"].ToString();

            if (!string.IsNullOrEmpty(variableName) && variableName.Any(char.IsWhiteSpace))
            {
                string errorMessage = errorMessageTemplate.Replace("{NAMEOFVAR}", variableName);
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }

        private bool EvaluateSec001(Dictionary<string, object> properties, StageContext context)
        {
            if (!string.Equals(context.Type, "Data", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            bool sensitiveName = context.Name != null &&
                (context.Name.ToLower().Contains("password") ||
                 context.Name.ToLower().Contains("username") ||
                 context.Name.ToLower().Contains("key"));

            if ((context.Datatype != null && context.Datatype.Equals("password", StringComparison.OrdinalIgnoreCase)) || sensitiveName)
            {
                if (!string.IsNullOrEmpty(context.InitialValue) || context.Private != true)
                {
                    string msg = properties["Error Message"].ToString().Replace("{NAMEOFVAR}", context.Name);
                    Console.WriteLine(msg);
                    return false;
                }
            }

            return true;
        }

        private bool EvaluateSec002(Dictionary<string, object> properties, StageContext context)
        {
            if (!string.Equals(context.Type, "Data", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(context.Exposure, "Environment", StringComparison.OrdinalIgnoreCase))
            {
                var name = context.Name?.ToLower() ?? string.Empty;
                if ((name.Contains("password") || name.Contains("key") || name.Contains("username")) && context.Private != true)
                {
                    string msg = properties["Error Message"].ToString().Replace("{NAMEOFVAR}", context.Name);
                    Console.WriteLine(msg);
                    return false;
                }
            }

            return true;
        }

        private bool EvaluateSec003(Dictionary<string, object> properties, StageContext context)
        {
            if (!string.Equals(context.Type, "Data", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(context.Exposure, "Environment", StringComparison.OrdinalIgnoreCase))
            {
                var name = context.Name?.ToLower() ?? string.Empty;
                bool isCred = name.Contains("password") || name.Contains("key") || name.Contains("username");
                if (isCred && !string.IsNullOrEmpty(context.InitialValue))
                {
                    string msg = properties["Error Message"].ToString().Replace("{NAMEOFVAR}", context.Name);
                    Console.WriteLine(msg);
                    return false;
                }
            }

            return true;
        }


        private bool EvaluateEnv001(Dictionary<string, object> properties, StageContext context)
        {
            if (!string.Equals(context.Type, "Data", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var value = context.InitialValue ?? string.Empty;
            bool isAbsolute = value.StartsWith("\\\\") || System.Text.RegularExpressions.Regex.IsMatch(value, @"^[A-Za-z]:\\");
            if (isAbsolute)
            {
                string msg = properties["Error Message"].ToString().Replace("{NAMEOFVAR}", context.Name);
                Console.WriteLine(msg);
                return false;
            }

            return true;
        }

        private bool EvaluateEnv002(Dictionary<string, object> properties, StageContext context)
        {
            if (!string.Equals(context.Type, "Data", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(context.Exposure, "Environment", StringComparison.OrdinalIgnoreCase))
            {
                string prefix = properties["Prefix"].ToString();
                if (!context.Name.StartsWith(prefix))
                {
                    string msg = properties["Error Message"].ToString()
                        .Replace("{NAMEOFVAR}", context.Name)
                        .Replace("{PREFIX}", prefix);
                    Console.WriteLine(msg);
                    return false;
                }
            }

            return true;
        }


        private bool EvaluateLog001(Dictionary<string, object> properties, StageContext context, Dictionary<string, object>? additionalProps)
        {
            if (!string.Equals(context.Type, "Action", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (additionalProps == null || !additionalProps.TryGetValue("Blocks", out var blocksObj) || blocksObj is not List<StageContext> blocks)
            {
                return true;
            }

            bool inside = blocks.Any(b => IsContextWithinBlock(context, b));
            if (!inside)
            {
                string msg = properties["Error Message"].ToString().Replace("{STAGENAME}", context.Name);
                Console.WriteLine(msg);
                return false;
            }

            return true;
        }

        private bool EvaluateLog002(Dictionary<string, object> properties, StageContext context)
        {
            if (string.IsNullOrEmpty(context.Name))
            {
                return true;
            }

            var lower = context.Name.ToLower();
            if (lower.Contains("recove") || lower.Contains("occurance"))
            {
                string msg = properties["Error Message"].ToString().Replace("{STAGENAME}", context.Name);
                Console.WriteLine(msg);
                return false;
            }

            return true;
        }

        private bool EvaluateLog003(Dictionary<string, object> properties, StageContext context)
        {
            if (string.IsNullOrEmpty(context.Name))
            {
                return true;
            }

            int maxLength = Convert.ToInt32(properties["Length"]);
            if (context.Name.Length > maxLength)
            {
                string msg = properties["Error Message"].ToString()
                    .Replace("{STAGENAME}", context.Name)
                    .Replace("{LENGTH}", maxLength.ToString());
                Console.WriteLine(msg);
                return false;
            }

            return true;
        }


        private bool EvaluateStage001(Dictionary<string, object> properties, StageContext context)
        {
            int requiredWordCount = Convert.ToInt32(properties["Word Count"]);
            var narrative = context.Narrative ?? string.Empty;
            int wordCount = narrative.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

            string errorMessage = properties["Error Message"].ToString()
                .Replace("{STAGENAME}", context.Name)
                .Replace("{PAGENAME}", context.PageName)
                .Replace("{WORDCOUNT}", requiredWordCount.ToString());

            if (wordCount < requiredWordCount)
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }

        private bool EvaluateStage002(Dictionary<string, object> properties, StageContext context, Dictionary<string, object>? additionalProps)
        {
            if (additionalProps == null || !additionalProps.TryGetValue("AllStages", out var stagesObj) || stagesObj is not List<StageContext> allStages)
            {
                throw new ArgumentException("AllStages not provided or invalid in additionalProps.");
            }

            bool hasRecover = allStages.Any(s => s.Type == "Recover" && s.PageName == context.PageName && IsContextWithinBlock(s, context));

            string errorMessage = properties["Error Message"].ToString()
                .Replace("{BLOCKNAME}", context.Name)
                .Replace("{PAGENAME}", context.PageName);

            if (!hasRecover)
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }

        private (string Name, string Color) ParseBlockProperties(object property)
        {
            var propertyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(property.ToString());
            return (propertyDict["Name"], propertyDict["Color"]);
        }
        private bool IsContextWithinBlock(StageContext context, StageContext block)
        {
            // Check geometrically if context is within the bounds of the block.
            return context.Display.X >= block.Display.X &&
                   context.Display.X <= block.Display.X + block.Display.Width &&
                   context.Display.Y >= block.Display.Y &&
                   context.Display.Y <= block.Display.Y + block.Display.Height;
        }


        public bool EvaluatePage001(Dictionary<string, object> properties, PageContext context)
        {
            int maxPageCount = Convert.ToInt32(properties["Count"]);
            string processName = context.ProcessName; // Assuming this is the process name.

            string errorMessageTemplate = properties["Error Message"].ToString();
            string errorMessage = errorMessageTemplate
                .Replace("{PROCESSNAME}", processName)
                .Replace("{MAXPAGES}", maxPageCount.ToString());

            if (context.Count > maxPageCount)
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }


        public bool EvaluatePage002(Dictionary<string, object> properties, PageContext context)
        {
            int requiredWordCount = Convert.ToInt32(properties["Word Count"]);
            string narrative = context.Narrative; // Assuming this is the page narrative/description.
            int actualWordCount = narrative?.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;

            // Updated error message template with more descriptive content
            string errorMessageTemplate = properties["Error Message"].ToString();
            string errorMessage = errorMessageTemplate
                .Replace("{PAGENAME}", context.Name)
                .Replace("{WORDCOUNT}", requiredWordCount.ToString())
                .Replace("{ACTUALWORDCOUNT}", actualWordCount.ToString());

            if (actualWordCount < requiredWordCount)
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }


        public bool EvaluatePage003(Dictionary<string, object> properties, PageContext context)
        {
            int requiredWordCount = Convert.ToInt32(properties["Word Count"]);

            // Assuming you have a way to extract preconditions and postconditions narrative for a page
            string preconditionsNarrative = context.Preconditions;
            string postconditionsNarrative = context.Postconditions;

            int preWordCount = preconditionsNarrative?.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            int postWordCount = postconditionsNarrative?.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;

            // Use the updated error message template
            string errorMessageTemplate = properties["Error Message"].ToString();
            string errorMessage = errorMessageTemplate
                .Replace("{PAGENAME}", context.Name)
                .Replace("{WORDCOUNT}", requiredWordCount.ToString())
                .Replace("{PREACTUALWORDCOUNT}", preWordCount.ToString())
                .Replace("{POSTACTUALWORDCOUNT}", postWordCount.ToString());

            if (preWordCount < requiredWordCount || postWordCount < requiredWordCount)
            {
                Console.WriteLine(errorMessage);
                return false;
            }

            return true;
        }




        private string StripDefinedCharacters(List<string> characters, string name)
        {
            if (characters == null) return name;
            foreach (var character in characters.Where(c => !string.IsNullOrEmpty(c)))
            {
                name = name.Replace(character, "");
            }
            return name;
        }

        private bool IsValidNamingConvention(List<string> conventions, string name)
        {
            // Simplified example, adapt as necessary
            return conventions.Contains("camelcase") && char.IsLower(name[0]) ||
                   conventions.Contains("pascalcase") && char.IsUpper(name[0]) ||
                   conventions.Contains("snakecase") && name.Contains('_');
        }


    }



}
