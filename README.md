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
- Read/write skills, agents, rules, and commands with YAML frontmatter
- View local and global configurations
- Deploy local-config to ~/.claude with automatic backup
- Import from ~/.claude back to local-config
- Diff calculator to preview changes
- Configuration validator for frontmatter and structure

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

## How to Deploy Your Configuration

### Using the Console Menu

1. **Run the application:**
   ```bash
   dotnet run --project "Claude Setup"
   ```

2. **View diff before deploying (option 6):**
   - See what will change when you deploy
   - Shows new, modified, and deleted files

3. **Validate configuration (option 7):**
   - Ensures all files have proper frontmatter
   - Checks required fields

4. **Deploy to ~/.claude (option 8):**
   - Copies all files from `local-config/` to `~/.claude/`
   - Creates automatic backup before deploying
   - Asks for confirmation

5. **Import from ~/.claude (option 9):**
   - Pulls changes from global back to local
   - Useful for syncing across machines

### Using the API

**View diff:**
```bash
curl http://localhost:5100/api/deploy/diff
```

**Validate configuration:**
```bash
curl http://localhost:5100/api/deploy/validate
```

**Deploy (with backup):**
```bash
curl -X POST "http://localhost:5100/api/deploy?backup=true&dryRun=false"
```

**Dry run (preview without deploying):**
```bash
curl -X POST "http://localhost:5100/api/deploy?backup=true&dryRun=true"
```

**Import from global:**
```bash
curl -X POST "http://localhost:5100/api/deploy/import?backup=true"
```

## API Endpoints

### Entities

- `GET /api/skills?isGlobal=false` - List skills from local-config or ~/.claude
- `GET /api/agents?isGlobal=false` - List agents
- `GET /api/rules?isGlobal=false` - List rules
- `GET /api/commands?isGlobal=false` - List commands

### Deployment

- `POST /api/deploy?backup=true&dryRun=false` - Deploy to ~/.claude
- `POST /api/deploy/import?backup=true` - Import from ~/.claude
- `GET /api/deploy/diff` - Compare local vs global
- `GET /api/deploy/validate` - Validate configuration

## CI/CD

GitHub Actions pipeline runs on every push and pull request:
- Restores dependencies
- Builds in Release configuration
- Runs all tests
- Publishes test results

## Roadmap

- [x] Diff calculator (compare local vs global) ✅
- [x] Configuration validator ✅
- [x] Backup strategy with timestamped backups ✅
- [x] Deploy configuration to ~/.claude ✅
- [x] Import configuration from ~/.claude ✅
- [x] Agents, Rules, Commands endpoints ✅
- [ ] Export/import as ZIP archive
- [ ] Edit entities via API (PUT/POST/DELETE endpoints)
- [ ] Merge conflict resolution for import
- [ ] Watch mode (auto-deploy on file change)

## License

MIT
