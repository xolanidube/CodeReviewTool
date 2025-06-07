using Newtonsoft.Json;
using RulesEngine;

namespace RulesEngine.Tests;

public class GeneralRuleEvaluatorTests
{
    private Dictionary<string, object> BuildVar001Properties()
    {
        return new Dictionary<string, object>
        {
            {"Prefixes", JsonConvert.SerializeObject(new List<string>{"in"})},
            {"Suffixes", JsonConvert.SerializeObject(new List<string>())},
            {"Characters", JsonConvert.SerializeObject(new List<string>{"_"})},
            {"HungarianNotation", JsonConvert.SerializeObject(new List<string>{"d"})},
            {"MultiwordDelimited", JsonConvert.SerializeObject(new List<string>{"camelcase"})},
            {"Error Message", "msg"}
        };
    }

    [Fact]
    public void EvaluateVar001_ValidName_ReturnsTrue()
    {
        var evaluator = new GeneralRuleEvaluator();
        var props = BuildVar001Properties();
        var context = new StageContext { Name = "indvalue" };

        var result = evaluator.Evaluate("VAR-001", props, context, null);

        Assert.True(result);
    }

    [Fact]
    public void EvaluateVar001_InvalidPrefix_ReturnsFalse()
    {
        var evaluator = new GeneralRuleEvaluator();
        var props = BuildVar001Properties();
        var context = new StageContext { Name = "xdvalue" };

        var result = evaluator.Evaluate("VAR-001", props, context, null);

        Assert.False(result);
    }
}
