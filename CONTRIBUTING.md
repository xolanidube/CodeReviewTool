# Contributing to CodeReviewTool

Thank you for considering contributing to **CodeReviewTool**! This project uses C# and targets .NET 8.0. This document outlines the basic conventions and workflow for contributing changes.

## Coding Conventions

- **Language**: The project is written in C#. Follow standard C# coding practices.
- **Indentation**: Use four spaces per indentation level. Do not use tabs.
- **Brace Style**: Place opening braces on the same line as the statement.
- **Naming**: Use `PascalCase` for class and method names, and `camelCase` for local variables and parameters.
- **File Organization**: Keep each class in its own file where practical and keep namespaces consistent with folder structure.
- **Dependencies**: Use the built-in .NET tooling (`dotnet` CLI) for restoring and building packages. Avoid committing binaries under `bin/` or `obj/`.

## Branching Strategy

1. **`main` branch**: Contains the latest stable code.
2. **Feature branches**: Create a new branch from `main` for your work. Use a descriptive name such as `feature/your-description` or `bugfix/issue-number`.
3. **Pull Requests**: When your feature or fix is ready, open a pull request (PR) targeting `main`. Ensure your branch is rebased or merged with the latest `main` before opening the PR.
4. **Commit Messages**: Write clear commit messages that explain the purpose of the change.

## Proposing Changes

1. **Open an issue**: If you are planning a significant change, open an issue first to discuss your proposal with the maintainers.
2. **Fork and clone**: Fork the repository on GitHub and clone your fork locally.
3. **Create a branch**: Create a feature branch as described above.
4. **Make your changes**: Follow the coding conventions. Include relevant documentation or comments.
5. **Test locally**: Run `dotnet build` to ensure the code compiles. Add tests if applicable.
6. **Submit a pull request**: Push your branch and open a PR on GitHub. Reference any related issues and provide a clear description of the change and testing steps.

We appreciate your contributions and look forward to collaborating!
