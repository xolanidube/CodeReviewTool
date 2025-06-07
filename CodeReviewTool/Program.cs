using System;
using System.IO;
using RulesEngine;

namespace CodeReviewTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: CodeReviewTool <target> <file> [rulesConfig]");
                Console.WriteLine("Targets: BluePrism, Python");
                return;
            }

            var target = args[0];
            var file = args[1];
            var configPath = args.Length > 2 ? args[2] : $"rulesConfig.{target.ToLower()}.json";

            var engine = new RulesEngine.RulesEngine();
            try
            {
                engine.LoadRuleConfig(configPath);
                engine.AddRulesFromConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load rule configuration: {ex.Message}");
                return;
            }

            try
            {
                if (target.Equals("BluePrism", StringComparison.OrdinalIgnoreCase))
                {
                    engine.Initialize(file);
                }
                else if (target.Equals("Python", StringComparison.OrdinalIgnoreCase))
                {
                    engine.LoadPythonFile(file);
                }
                else
                {
                    Console.WriteLine($"Unsupported target: {target}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load input file: {ex.Message}");
                return;
            }

            engine.ValidateAll();
        }
    }
}
