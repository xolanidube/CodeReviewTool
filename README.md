# CodeReviewTool

A .NET rule engine for validating Blue Prism process files.

See [docs/rules.md](docs/rules.md) for details on the rule configuration format and evaluator behavior.

## API usage

Run the `CodeReviewTool.Api` project to expose REST endpoints for validating
Blue Prism files.

### Validate a process

```
POST /api/validate/process?config=rulesConfig.json
```

Upload the `.bpprocess` file using the field name `process`.

### Validate a business object

```
POST /api/validate/object?config=rulesConfig.json
```

Upload the `.bpobject` file using the field name `object`.

In both cases the optional `config` query parameter specifies which rules
configuration JSON file to load. If omitted, `rulesConfig.json` in the
application directory is used. The response is a JSON array of validation
messages.
