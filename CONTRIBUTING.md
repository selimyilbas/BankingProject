# Contributing to VakıfBank Banking Application

First off, thank you for considering contributing to this project! 

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples**
- **Describe the behavior you observed and expected**
- **Include screenshots if possible**
- **Include your environment details** (OS, .NET version, Node version, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description of the suggested enhancement**
- **Provide specific examples to demonstrate the enhancement**
- **Describe the current behavior and expected behavior**
- **Explain why this enhancement would be useful**

### Pull Requests

1. Fork the repo and create your branch from `main`
2. If you've added code that should be tested, add tests
3. Ensure the test suite passes
4. Make sure your code follows the existing code style
5. Issue that pull request!

## Development Process

1. **Setup your development environment** following the README instructions
2. **Create a feature branch**: `git checkout -b feature/your-feature-name`
3. **Make your changes** following the coding standards below
4. **Test your changes** thoroughly
5. **Commit your changes**: `git commit -m 'Add some feature'`
6. **Push to the branch**: `git push origin feature/your-feature-name`
7. **Submit a pull request**

## Coding Standards

### C# / .NET

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods small and focused on a single responsibility
- Use async/await for asynchronous operations
- Handle exceptions appropriately

### Angular / TypeScript

- Follow the [Angular Style Guide](https://angular.io/guide/styleguide)
- Use standalone components (no modules)
- Use TypeScript strict mode
- Implement proper error handling
- Write meaningful component and service names
- Keep components focused and small

### General

- Write self-documenting code
- Add comments only when necessary
- Keep the code DRY (Don't Repeat Yourself)
- Test your code before submitting
- Update documentation if needed

## Project Structure

Please maintain the existing project structure:

```
BankingApp/
├── BankingApp.API/          # Keep controllers thin
├── BankingApp.Application/  # Business logic goes here
├── BankingApp.Domain/       # Domain entities and interfaces
├── BankingApp.Infrastructure/ # Data access implementation
└── BankingApp.Common/       # Shared utilities

BankingApp.UI/
└── src/app/
    ├── components/  # UI components
    ├── services/    # Angular services
    └── models/      # TypeScript interfaces
```

## Commit Messages

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters or less
- Reference issues and pull requests liberally after the first line

### Commit Message Format

```
<type>: <subject>

<body>

<footer>
```

Types:
- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that don't affect the code meaning
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **perf**: Code change that improves performance
- **test**: Adding missing tests
- **chore**: Changes to the build process or auxiliary tools

## Questions?

Feel free to open an issue with your question or contact the maintainers directly.