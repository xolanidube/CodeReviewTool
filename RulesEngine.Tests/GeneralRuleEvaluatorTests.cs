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

    private Dictionary<string, object> BuildSec001Properties()
    {
        return new Dictionary<string, object>
        {
            {"Error Message", "msg"}
        };
    }

    [Fact]
    public void EvaluateSec001_PublicPassword_ReturnsFalse()
    {
        var evaluator = new GeneralRuleEvaluator();
        var props = BuildSec001Properties();
        var context = new StageContext { Type = "Data", Name = "Password", Datatype = "password", Private = false, InitialValue = "123" };

        var result = evaluator.Evaluate("SEC-001", props, context, null);

        Assert.False(result);
    }

    private Dictionary<string, object> BuildEnv001Properties()
    {
        return new Dictionary<string, object>
        {
            {"Error Message", "msg"}
        };
    }

    [Fact]
    public void EvaluateEnv001_AbsolutePath_ReturnsFalse()
    {
        var evaluator = new GeneralRuleEvaluator();
        var props = BuildEnv001Properties();
        var context = new StageContext { Type = "Data", Name = "File", InitialValue = "C:\\temp\\file.txt" };

        var result = evaluator.Evaluate("ENV-001", props, context, null);

        Assert.False(result);
    }

    private Dictionary<string, object> BuildPy001Properties()
    {
        return new Dictionary<string, object>
        {
            {"Error Message", "msg"}
        };
    }

    [Fact]
    public void EvaluatePy001_EvalUsed_ReturnsFalse()
    {
        var evaluator = new GeneralRuleEvaluator();
        var props = BuildPy001Properties();
        var context = new PythonContext { FilePath = "test.py", Code = "eval('1+1')" };

        var result = evaluator.Evaluate("PY-001", props, context, null);

        Assert.False(result);
    }

    private Dictionary<string, object> BuildPy002Properties()
    {
        return new Dictionary<string, object>
        {
            {"Error Message", "msg"}
        };
    }

    [Fact]
    public void EvaluatePy002_TodoComment_ReturnsFalse()
    {
        var evaluator = new GeneralRuleEvaluator();
        var props = BuildPy002Properties();
        var context = new PythonContext { FilePath = "test.py", Code = "# TODO: fix" };

        var result = evaluator.Evaluate("PY-002", props, context, null);

        Assert.False(result);
    }
}
