using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RulesEngine
{
    public class Contexts
    {
        // Dictionary to hold lists of contexts by their type
        private Dictionary<Type, IList> contextLists = new Dictionary<Type, IList>();

        public Contexts() { }

        public void AddContext<T>(T context) where T : class
        {
            Type type = typeof(T);
            if (!contextLists.ContainsKey(type))
            {
                contextLists[type] = new List<T>();
            }
            ((List<T>)contextLists[type]).Add(context);
        }

        public IEnumerable<T> GetContexts<T>() where T : class
        {
            Type type = typeof(T);
            if (contextLists.ContainsKey(type))
            {
                return (List<T>)contextLists[type];
            }
            return new List<T>();
        }

        // Dynamically call extraction methods based on the types present in the assembly
        // This could be called to populate contexts from an XML element
        public void LoadContexts(XElement xmlContent)
        {
            

            // Assuming naming convention: "Extract" + type name + "s" (e.g., ExtractStageContexts)
            // Adjusted to work with instance methods
            var contextTypes = this.GetType().Assembly.GetTypes()
                .Where(t => typeof(Contexts).IsAssignableFrom(t) && !t.IsAbstract && t != typeof(Contexts)); // Exclude the base class itself


            foreach (var contextType in contextTypes)
            {
                // Create an instance of each context type
                var contextInstance = Activator.CreateInstance(contextType) as Contexts;
                if (contextInstance != null)
                {
                    var methodName = $"Extract{contextType.Name}s";
                    var method = contextType.GetMethod(methodName, new[] { typeof(XElement) });
                    if (method != null)
                    {
                        var extractedContexts = method.Invoke(contextInstance, new object[] { xmlContent }) as IList;
                        if (extractedContexts != null)
                        {
                            foreach (var context in extractedContexts)
                            {
                                // Use the method MakeGenericMethod to invoke AddContext with the correct type
                                var addContextMethod = typeof(Contexts).GetMethod(nameof(AddContext), BindingFlags.Public | BindingFlags.Instance);
                                var genericAddContextMethod = addContextMethod.MakeGenericMethod(context.GetType());
                                genericAddContextMethod.Invoke(this, new object[] { context });
                            }
                        }
                    }
                }
            }
        }
    }

    public class StageContext : Contexts
    {
        public string? StageId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Exposure { get; set; }

        public string? Narrative { get; set; }
        public bool? LoginInhibit { get; set; }
        public string? InitialValue { get; set; }

        public string? Datatype { get; set;}
        public bool? AlwaysInit { get; set; }
        public bool? Private { get; set; }

        public List<string?> Inputs { get; set; } = new List<string?>();
        public List<string?> Outputs { get; set; } = new List<string?>();

        public Display Display { get; set; }

        public Font Font { get; set; }

        public string? PageName { get; set; }

        public StageContext()
        {
        }

        public StageContext(string stageId, string name, string type, string exposure, string narrative, bool? loginInhibit, string initialValue, bool? alwaysInit, bool? isPrivate)
        {
            StageId = stageId;
            Name = name;
            Type = type;
            Exposure = exposure;
            Narrative = narrative;
            LoginInhibit = loginInhibit;
            InitialValue = initialValue;
            AlwaysInit = alwaysInit;
            Private = isPrivate;
        }

        public List<StageContext> ExtractStageContexts(XElement xmlContent)
        {
            // First, create a dictionary mapping subsheet IDs to page names for easy lookup
            var pageNamesBySubsheetId = xmlContent.Descendants("subsheet")
                .ToDictionary(
                    subsheet => (string)subsheet.Attribute("subsheetid"),
                    subsheet => (string)subsheet.Element("name")
                );

            var stages = xmlContent.Descendants("stage")
                .Select(stage =>
                {
                    var context = new StageContext
                    {
                        StageId = (string)stage.Attribute("stageid"),
                        Name = (string)stage.Attribute("name"),
                        Type = (string)stage.Attribute("type"),
                        Exposure = stage.Elements("exposure").FirstOrDefault()?.Value,
                        Narrative = stage.Elements("narrative").FirstOrDefault()?.Value,
                        LoginInhibit = stage.Elements("loginhibit").FirstOrDefault() != null ? (bool?)stage.Element("loginhibit").Attribute("onsuccess") : null,
                        InitialValue = stage.Elements("initialvalue").FirstOrDefault()?.Value,
                        AlwaysInit = stage.Elements("alwaysinit").Any(),
                        Private = stage.Elements("private").Any(),
                        Datatype = stage.Elements("datatype").FirstOrDefault()?.Value,
                        Display = new Display(
                            (int?)stage.Element("display")?.Attribute("x"),
                            (int?)stage.Element("display")?.Attribute("y"),
                            (int?)stage.Element("display")?.Attribute("h"),
                            (int?)stage.Element("display")?.Attribute("w")
                        ),
                        Font = new Font(
                            (string)stage.Element("font")?.Attribute("family"),
                            (int?)stage.Element("font")?.Attribute("size"),
                            (string)stage.Element("font")?.Attribute("style"),
                            (string)stage.Element("font")?.Attribute("color")
                        ),
                        Inputs = stage.Elements("inputs").Descendants("input").Select(input => (string)input.Attribute("stage")).ToList(),
                        Outputs = stage.Elements("outputs").Descendants("output").Select(output => (string)output.Attribute("stage")).ToList()
                    };

                    // Find and assign the PageName using the subsheetid
                    var subsheetId = (string)stage.Element("subsheetid");
                    if (subsheetId != null && pageNamesBySubsheetId.TryGetValue(subsheetId, out var pageName))
                    {
                        context.PageName = pageName;
                    }
                    else
                    {
                        context.PageName = "Main Page";
                    }

                    return context;
                })
            .ToList();

            return stages;
        }

        public List<StageContext> ExtractStageContexts(XElement xmlContent, string filterType = null)
        {
            var stages = xmlContent.Descendants("stage")
                .Select(stage =>
                {
                    var context = new StageContext
                    {
                        StageId = (string)stage.Attribute("stageid"),
                        Name = (string)stage.Attribute("name"),
                        Type = (string)stage.Attribute("type"),
                        Exposure = stage.Elements("exposure").FirstOrDefault() != null ? (string)stage.Element("exposure") : null,
                        Narrative = stage.Elements("narrative").FirstOrDefault() != null ? (string)stage.Element("narrative") : "",
                        LoginInhibit = stage.Elements("loginhibit").FirstOrDefault() != null ? (bool?)stage.Element("loginhibit").Attribute("onsuccess") : null,
                        InitialValue = stage.Elements("initialvalue").FirstOrDefault() != null ? (string)stage.Element("initialvalue") : "",
                        AlwaysInit = stage.Elements("alwaysinit").Any(),
                        Private = stage.Elements("private").Any()
                    };

                    // Check if inputs are present and then extract, otherwise initialize to empty list
                    var inputs = stage.Descendants("input");
                    if (inputs != null && inputs.Any())
                    {
                        context.Inputs = inputs.Select(input => (string)input.Attribute("stage")).ToList();
                    }
                    else
                    {
                        context.Inputs = new List<string>();
                    }

                    // Assuming outputs processing similar to inputs
                    var outputs = stage.Descendants("output");
                    if (outputs != null && outputs.Any())
                    {
                        context.Outputs = outputs.Select(output => (string)output.Attribute("stage")).ToList();
                    }
                    else
                    {
                        context.Outputs = new List<string>();
                    }

                    return context;
                })
            .ToList();



            return stages;
        }

    }

    public class PageContext : Contexts
    {
        public string? Id { get; set; }
        
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Published { get; set; }
        public string? Narrative { get; set; }
        public int? Count { get; set; }

        public string? Preconditions { get; set; }
        public string? Postconditions { get; set; }
        public string? ProcessName { get; set; }

        public PageContext()
        {
        }

        public PageContext(string? id, string? name, string? type, string? published, string? narrative)
        {
            Id = id;
            Name = name;
            Type = type;
            Published = published;
            Narrative = narrative;
        }

        public List<PageContext> ExtractPageContexts(XElement xmlContent)
        {
            var pageContexts = new List<PageContext>();

            var processName = (string)xmlContent.Attribute("name");

            var subsheets = xmlContent.Descendants("subsheet");
            foreach (var subsheet in subsheets)
            {
                var subsheetId = (string)subsheet.Attribute("subsheetid");
                var pageContext = new PageContext
                {
                    Id = subsheetId,
                    Name = (string)subsheet.Element("name"),
                    Type = (string)subsheet.Attribute("type"),
                    Published = (string)subsheet.Attribute("published")
                };

                pageContext.ProcessName = processName;

                // Find the narrative by matching subsheetid with stage's subsheetid
                var narrativeStage = xmlContent.Descendants("stage")
                    .FirstOrDefault(s => (string)s.Element("subsheetid") == subsheetId && (string)s.Attribute("type") == "SubSheetInfo");
                pageContext.Narrative = (string)narrativeStage?.Element("narrative");

                // Find stages associated with this subsheet to extract preconditions and postconditions
                var conditionStages = xmlContent.Descendants("stage")
                    .Where(stage => (string)stage.Element("subsheetid") == subsheetId && (stage.Attribute("type").Value == "Start" || stage.Attribute("type").Value == "End"));

                foreach (var stage in conditionStages)
                {
                    var preconditions = stage.Elements("preconditions").Elements("condition")
                        .Select(cond => cond.Attribute("narrative").Value).FirstOrDefault();
                    var postconditions = stage.Elements("postconditions").Elements("condition")
                        .Select(cond => cond.Attribute("narrative").Value).FirstOrDefault();

                    // Only set preconditions and postconditions if not already set (assuming only one Start and End stage per subsheet for simplicity)
                    pageContext.Preconditions = pageContext.Preconditions ?? preconditions;
                    pageContext.Postconditions = pageContext.Postconditions ?? postconditions;
                }

                pageContexts.Add(pageContext);
            }

            Count = pageContexts.Count;

            return pageContexts;
        }

    }

}
