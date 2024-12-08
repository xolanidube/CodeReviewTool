using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using RulesEngine;


namespace CodeReviewTool
{

    


    internal class Program
    {

        

        static void Main(string[] args)
        {
            var engine = new RulesEngine.RulesEngine();
            engine.LoadRuleConfig("rulesConfig.json");
            engine.AddRulesFromConfig();
            engine.Initialize(@"C:\Users\Xolan\Downloads\Processes\BPA Process - RBBAT10SS_Process Card and Card Blocks and Holds Closures.bpprocess");
            engine.ValidateAll();

            // Load the XML content
            //var xmlContent = XElement.Load(@"C:\Users\Xolan\Downloads\Processes\BPA Process - Variable Rules.bpprocess");

            //var stageContexts = stageContext.ExtractStageContexts(xmlContent, "Data")
            //.Where(s => !string.IsNullOrEmpty(s.Exposure))
            //.ToList();

            // Register the general rule evaluator
            //engine.RegisterEvaluator(Rule.Id , new GeneralRuleEvaluator());

            // Evaluate rules against contexts
            /*foreach (var sContext in stageContexts)
            {
                // Adapt this to loop through all applicable rules instead of hardcoding "VAR-002"
                bool result = engine.Evaluate("VAR-002", sContext);
                //Console.WriteLine(result ? "Variable name is valid." : "Variable name is invalid.");
            }*/

            Console.ReadLine();
        }
    }
}
