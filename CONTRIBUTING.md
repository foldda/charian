# Contributing

Thanks for your interest in contributing to project Charian - one of Foldda's open-source technology building blocks. We welcome issues, discussions, and pull requests from the community.

## Ways to contribute

- **Report a bug** — open an issue with steps to reproduce, what you expected, and what actually happened. Include your OS, .NET version, and package version if relevant.
- **Suggest an enhancement** — open an issue describing the use case and why it's not covered by the current design. Design discussion up front saves rework later, especially for anything touching the public API.
- **Improve documentation** — typo fixes, clearer examples, and missing edge cases are all welcome; no issue required for small doc PRs.
- **Submit code** — see the workflow below.

## Before you start on a code change

For anything beyond a small fix, please open an issue (or comment on an existing one) describing what you plan to do *before* writing the PR. This project is maintained by a small team, and confirming direction first avoids spending your time on something that doesn't get merged.

## Development workflow

1. Fork the repo and create a branch from `main`: `git checkout -b fix/short-description`.
2. Make your changes, keeping commits focused and logically separated.
3. Add or update tests for any behavior change. PRs without test coverage for new logic will likely be asked to add it.
4. Make sure the existing test suite passes locally before opening the PR.
5. Open a pull request against `main` with:
   - A clear description of *what* changed and *why*.
   - A reference to the related issue, if any (`Fixes #123`).
   - Any notes on backward compatibility, since this library is used in downstream production systems.

## Code style

- Follow the existing style and structure already used in the codebase rather than introducing a new convention — consistency matters more than personal preference here.
- Use standard C# naming conventions (PascalCase for public members, camelCase for locals/private fields).
- Keep public API changes minimal and intentional; this library is designed to be stable for consumers who depend on it.
- Prefer small, single-purpose pull requests over large multi-feature ones — they're easier to review and merge quickly.

## Commit messages

Use a short, descriptive summary line (50 characters or fewer where possible), followed by a blank line and more detail if needed. Reference issue numbers where relevant.

## License

By contributing, you agree that your contributions will be licensed under the same [MIT License](LICENSE) that covers this project, with copyright held by Foldda Pty Ltd.

## Code of conduct

Be respectful and constructive. Disagreements on technical direction are fine and expected; personal attacks, harassment, or bad-faith engagement are not, and may result in comments or PRs being closed.

## Questions

If you're not sure where something belongs, or want to discuss an idea before formalizing it as an issue, open a [Discussion](../../discussions) or reach out via contact@foldda.com.
