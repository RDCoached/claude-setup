# Claude Configuration Manager

![CI](https://github.com/RDCoached/claude-setup/workflows/CI/badge.svg)

A .NET 10 console application with embedded Web API to manage Claude configuration files (skills, agents, rules, commands). Maintains a local working copy of configuration and can deploy changes to the global `~/.claude/` directory.

## Features

✅ **Dual-Mode Operation**
- Interactive console menu for local use
- Embedded Web API with Swagger UI
- Background API + console (default)
- API-only mode with `--api` flag

✅ **Configuration Management**
- Read/write skills with YAML frontmatter
- Read/write agents, rules, and commands
- View local and global configurations
- Future: Deploy, backup, diff, and validation

✅ **Well-Tested**
- 31 passing tests
- TDD approach throughout
- Integration and unit tests

## Requirements

- .NET 10 SDK

## Getting Started

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run Application

**Console mode (default):**
```bash
dotnet run --project "Claude Setup"
```

**API-only mode:**
```bash
dotnet run --project "Claude Setup" -- --api
```

The API runs on http://localhost:5100
Swagger UI: http://localhost:5100/swagger

## Project Structure

```
Claude Setup/
├── Domain/Models/          # Domain entities (Skill, Agent, Rule, Command)
├── Infrastructure/
│   ├── Configuration/      # Path resolution
│   └── FileSystem/         # Frontmatter parser, file reader/writer
├── Features/
│   ├── Skills/             # Skill management endpoints and handlers
│   └── Console/            # Interactive menu
└── Program.cs              # Entry point, DI setup

ClaudeSetup.Tests/
├── Infrastructure/         # Infrastructure tests
├── Features/               # Feature tests
└── TestFixtures/           # Test helpers

local-config/               # Local working directory
├── skills/
├── agents/
├── rules/
└── commands/
```

## Architecture

- **Vertical Slice Architecture** - Features organized by capability
- **TDD** - All code written test-first
- **Primary Constructors** - DI via constructor injection
- **Records** - Immutable domain models
- **Minimal API** - Lightweight HTTP endpoints
- **Auto-Discovery** - Endpoints registered via `IEndpointGroup` reflection

## API Endpoints

### Skills

- `GET /api/skills?isGlobal=false` - List skills from local-config or ~/.claude

## CI/CD

GitHub Actions pipeline runs on every push and pull request:
- Restores dependencies
- Builds in Release configuration
- Runs all tests
- Publishes test results

## Roadmap

- [ ] Diff calculator (compare local vs global)
- [ ] Configuration validator
- [ ] Backup strategy with timestamped backups
- [ ] Deploy configuration to ~/.claude
- [ ] Import configuration from ~/.claude
- [ ] Agents, Rules, Commands endpoints
- [ ] Export/import as ZIP archive

## License

MIT
