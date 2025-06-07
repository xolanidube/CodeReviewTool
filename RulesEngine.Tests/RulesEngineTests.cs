using RulesEngine;

namespace RulesEngine.Tests;

public class RulesEngineTests
{
    [Fact]
    public void AddRule_StoresProperties()
    {
        var engine = new global::RulesEngine.RulesEngine();
        var props = new Dictionary<string, object> { { "test", "value" } };

        engine.AddRule("RULE-1", props);

        Assert.True(engine._rules.ContainsKey("RULE-1"));
        Assert.Equal("value", engine._rules["RULE-1"]["test"]);
    }

    [Fact]
    public void RegisterEvaluator_StoresEvaluator()
    {
        var engine = new global::RulesEngine.RulesEngine();
        var evaluator = new GeneralRuleEvaluator();

        engine.RegisterEvaluator("RULE-1", evaluator);

        Assert.True(engine._evaluators.ContainsKey("RULE-1"));
        Assert.Same(evaluator, engine._evaluators["RULE-1"]);
    }

    [Fact]
    public void Evaluate_InvokesRegisteredEvaluator()
    {
        var engine = new global::RulesEngine.RulesEngine();
        var props = new Dictionary<string, object>();
        engine.AddRule("RULE-1", props);

        var mockEval = new TestEvaluator();
        engine.RegisterEvaluator("RULE-1", mockEval);

        var result = engine.Evaluate("RULE-1", new object(), null);

        Assert.True(mockEval.Called);
        Assert.True(result);
    }

    private class TestEvaluator : IRuleEvaluator
    {
        public bool Called { get; private set; }
        public bool Evaluate(string ruleId, Dictionary<string, object> ruleProperties, object context, Dictionary<string, object>? additionalProperties)
        {
            Called = true;
            return true;
        }
    }
}
