# CodeReviewTool


This repository contains a simple rules engine and a console application used to validate Blue Prism processes. Unit tests are written using **xUnit**.

## Running Tests

Execute the following command from the repository root to run all tests:

```bash
dotnet test
```

## Command line usage
Run the console with:
```bash
CodeReviewTool <target> <file> [rulesConfig]
```
The default rules configuration is `rulesConfig.<target>.json`. Targets supported: `BluePrism`, `Python`.


A .NET rule engine for validating Blue Prism and Python code. Specify the target when running.

See [docs/rules.md](docs/rules.md) for details on the rule configuration format and evaluator behavior.

## API usage

Run the `CodeReviewTool.Api` project to expose a REST endpoint for validating
Blue Prism `.bpprocess` files. The service provides a single endpoint:

```
POST /api/validate
```

Upload the process file as multipart/form-data using the field name `process`.
The response is a JSON array of validation messages.

