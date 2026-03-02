# Contributing to Mini Trading Platform

Thank you for taking the time to contribute! Please read the guidelines below before opening a pull request.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Branching Strategy](#branching-strategy)
- [Commit Messages](#commit-messages)
- [Code Style](#code-style)
- [Pull Request Checklist](#pull-request-checklist)
- [Reporting Issues](#reporting-issues)

---

## Code of Conduct

Be respectful and constructive. This is an educational project — questions and suggestions are always welcome.

---

## Getting Started

1. Fork the repository.
2. Clone your fork: `git clone https://github.com/<your-username>/Mini_Trading_platform.git`
3. Set the upstream remote: `git remote add upstream https://github.com/nvm-my/Mini_Trading_platform.git`
4. Create a feature branch (see [Branching Strategy](#branching-strategy)).

---

## Branching Strategy

| Branch prefix | Purpose |
|---|---|
| `feature/<short-description>` | New functionality |
| `fix/<short-description>` | Bug fixes |
| `docs/<short-description>` | Documentation-only changes |
| `chore/<short-description>` | Tooling, config, CI changes |
| `refactor/<short-description>` | Code restructuring without behaviour change |

Always branch off `main` and target `main` when opening a PR.

---

## Commit Messages

Follow the [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>(<scope>): <short summary>

[optional body]

[optional footer]
```

Examples:

```
feat(orders): add limit order price validation
fix(auth): handle null user in Login endpoint
docs(readme): update configuration section
chore(editorconfig): add C# naming rules
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`.

---

## Code Style

This project uses `.editorconfig` to enforce consistent formatting across all C# files. Before committing, please run:

```bash
dotnet format Mini/TradingPlatformAPI/TradingPlatformAPI.csproj
```

### Key conventions

| Rule | Value |
|---|---|
| Indentation | 4 spaces (no tabs) |
| `var` usage | Prefer explicit types for built-in types; `var` when the type is obvious |
| Braces | Always use braces for control flow blocks |
| Private fields | `_camelCase` prefix |
| Interfaces | `IPascalCase` prefix |
| Public members | `PascalCase` |
| Namespace style | File-scoped namespaces (`namespace Foo.Bar;`) |
| Using directives | `System.*` first, then alphabetical, then a blank line before other imports |
| Trailing whitespace | Stripped automatically by `.editorconfig` |

### Analyzers

`Directory.Build.props` enables the .NET Recommended analyzer set for all projects. Resolve any new diagnostics introduced by your changes before submitting.

---

## Pull Request Checklist

Before opening a PR, ensure all of the following are true:

- [ ] The branch is up-to-date with `main` (rebase or merge).
- [ ] `dotnet build` succeeds with no errors.
- [ ] `dotnet format --verify-no-changes` reports no formatting issues.
- [ ] All new business logic is covered by at least one unit test (when a test project exists).
- [ ] No secrets, connection strings, or API keys are committed.
- [ ] The PR description explains **what** changed and **why**.
- [ ] Relevant documentation (README, XML doc comments) has been updated.

---

## Reporting Issues

- Check existing issues before opening a new one.
- Include steps to reproduce, expected behaviour, and actual behaviour.
- Attach relevant logs or screenshots where helpful.
