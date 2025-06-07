# Rules Configuration

The `rulesConfig.<target>.json` file (e.g., `rulesConfig.blueprism.json` or `rulesConfig.python.json`) defines how the `RulesEngine` validates a process. Rules are organized into **rule groups** under the `RuleGroups` section. Each group contains a `Rules` object mapping a rule ID to its properties.

Example structure:

```json
{
  "RuleGroups": {
    "Variables": {
      "Rules": {
        "VAR-001": { "Description": "..." }
      }
    },
    "Pages": { "Rules": { "PAGE-001": { } } }
  }
}
```

The classes `RuleConfig` and `RuleGroup` in `RulesEngine/RuleConfig.cs` represent this layout in code.

## Variables

Rules in the **Variables** group validate naming and placement of variables. Common fields include:

- `Description` – explains what the rule checks.
- `Characters`, `Prefixes`, `Suffixes` – lists used to build naming conventions.
- `MultiwordDelimited`, `HungarianNotation` – allowed casing or notation styles.
- `Start Stage Allowed Prefixes`, `End Stage Allowed Prefixes` – restrict where variables can be declared.
- `Length` – maximum allowed variable length (VAR‑004).
- `Error Message` – text shown when validation fails. Placeholders such as `{NAMEOFVAR}` or `{EXPECTEDPREFIXES}` are replaced at runtime.
- `Active` – set to `true` to enable the rule.

### Adding a new variable rule

Add a new rule ID under `Variables -> Rules` in `rulesConfig.<target>.json`:

```json
"VAR-006": {
  "Description": "Checks variable placement within color-coded blocks.",
  "Environment Variables Color": { "Name": "Environment", "Color": "Blue" },
  "Error Message": "Variable '{NAMEOFVAR}' is not inside the correct block.",
  "Active": true
}
```

Implement the corresponding method (e.g. `EvaluateVar006`) in `GeneralRuleEvaluator`. The engine loads new rules with `AddRulesFromConfig()` and applies them when `ValidateAll()` is called.

## Pages

The **Pages** group contains rules that operate on page contexts. Fields include:

- `Description` – summary of the rule.
- `Count` – maximum number of pages allowed (PAGE‑001).
- `Word Count` – minimum description length (PAGE‑002).
- `Error Message` and `Active` – behave the same as in the Variables group.

## Evaluator behavior

When `ValidateAll()` runs, each active rule is evaluated against the appropriate context. `GeneralRuleEvaluator` selects a method based on the rule ID (e.g. `EvaluateVar001`, `EvaluatePage001`). Custom evaluators can be registered with `RulesEngine.RegisterEvaluator` if needed.
