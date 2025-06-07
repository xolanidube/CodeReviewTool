# CodeReviewTool

A .NET rule engine for validating Blue Prism process files.

See [docs/rules.md](docs/rules.md) for details on the rule configuration format and evaluator behavior.

## API usage

Run the `CodeReviewTool.Api` project to expose a REST endpoint for validating
Blue Prism `.bpprocess` files. The service provides a single endpoint:

```
POST /api/validate
```

Upload the process file as multipart/form-data using the field name `process`.
The response is a JSON array of validation messages.
