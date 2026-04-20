using Claude_Setup.Domain.Models;
using Claude_Setup.Domain.Services;
using Claude_Setup.Features.Agents;
using Claude_Setup.Features.Commands;
using Claude_Setup.Features.Deploy;
using Claude_Setup.Features.Rules;
using Claude_Setup.Features.Skills;
using Claude_Setup.Infrastructure.Configuration;

namespace Claude_Setup.Features.Console;

public sealed class InteractiveMenu(
    ListSkills listSkills,
    ListAgents listAgents,
    ListRules listRules,
    ListCommands listCommands,
    DeployConfiguration deployConfig,
    ImportConfiguration importConfig,
    DiffCalculator diffCalculator,
    ConfigurationValidator validator,
    ClaudePathResolver pathResolver)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DisplayMenu();
            var choice = System.Console.ReadLine()?.Trim();

            try
            {
                await HandleChoiceAsync(choice, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nError: {ex.Message}\n");
            }

            if (choice == "0")
            {
                break;
            }
        }
    }

    private static void DisplayMenu()
    {
        System.Console.WriteLine("\n╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║   Claude Configuration Manager             ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("  📋 View");
        System.Console.WriteLine("  1. Skills (Local)");
        System.Console.WriteLine("  2. Skills (Global ~/.claude)");
        System.Console.WriteLine("  3. Agents (Local)");
        System.Console.WriteLine("  4. Rules (Local)");
        System.Console.WriteLine("  5. Commands (Local)");
        System.Console.WriteLine();
        System.Console.WriteLine("  🔄 Deploy");
        System.Console.WriteLine("  6. View Diff (Local vs Global)");
        System.Console.WriteLine("  7. Validate Configuration");
        System.Console.WriteLine("  8. Deploy (Dry Run / Preview)");
        System.Console.WriteLine("  9. Deploy to ~/.claude (with backup)");
        System.Console.WriteLine("  10. Import from ~/.claude (pull changes)");
        System.Console.WriteLine();
        System.Console.WriteLine("  0. Exit");
        System.Console.WriteLine();
        System.Console.Write("Choose an option: ");
    }

    private async Task HandleChoiceAsync(string? choice, CancellationToken cancellationToken)
    {
        switch (choice)
        {
            case "1":
                await ViewSkillsAsync(isGlobal: false);
                break;

            case "2":
                await ViewSkillsAsync(isGlobal: true);
                break;

            case "3":
                await ViewAgentsAsync();
                break;

            case "4":
                await ViewRulesAsync();
                break;

            case "5":
                await ViewCommandsAsync();
                break;

            case "6":
                await ViewDiffAsync();
                break;

            case "7":
                await ValidateAsync();
                break;

            case "8":
                await DeployDryRunAsync();
                break;

            case "9":
                await DeployAsync();
                break;

            case "10":
                await ImportAsync();
                break;

            case "0":
                System.Console.WriteLine("\nGoodbye!\n");
                break;

            default:
                System.Console.WriteLine("\nInvalid choice. Please try again.\n");
                break;
        }
    }

    private async Task ViewSkillsAsync(bool isGlobal)
    {
        var location = isGlobal ? "Global (~/.claude)" : "Local (local-config)";
        System.Console.WriteLine($"\n{location} Skills:\n");

        var skills = await listSkills.HandleAsync(isGlobal);

        if (skills.Count == 0)
        {
            System.Console.WriteLine("  (No skills found)\n");
            return;
        }

        foreach (var skill in skills)
        {
            System.Console.WriteLine($"  • {skill.Name}");
            System.Console.WriteLine($"    {skill.Description}");
            System.Console.WriteLine($"    Path: {skill.FolderPath}");
            System.Console.WriteLine();
        }

        System.Console.WriteLine($"Total: {skills.Count} skill(s)\n");
    }

    private async Task ViewAgentsAsync()
    {
        System.Console.WriteLine("\nLocal Agents:\n");

        var agents = await listAgents.HandleAsync(isGlobal: false);

        if (agents.Count == 0)
        {
            System.Console.WriteLine("  (No agents found)\n");
            return;
        }

        foreach (var agent in agents)
        {
            System.Console.WriteLine($"  • {agent.Name}");
            if (agent.Role is not null)
            {
                System.Console.WriteLine($"    Role: {agent.Role}");
            }
            System.Console.WriteLine($"    Path: {agent.FilePath}");
            System.Console.WriteLine();
        }

        System.Console.WriteLine($"Total: {agents.Count} agent(s)\n");
    }

    private async Task ViewRulesAsync()
    {
        System.Console.WriteLine("\nLocal Rules:\n");

        var rules = await listRules.HandleAsync(isGlobal: false);

        if (rules.Count == 0)
        {
            System.Console.WriteLine("  (No rules found)\n");
            return;
        }

        foreach (var rule in rules)
        {
            System.Console.WriteLine($"  • {rule.Name}");
            System.Console.WriteLine($"    Path: {rule.FilePath}");
            System.Console.WriteLine();
        }

        System.Console.WriteLine($"Total: {rules.Count} rule(s)\n");
    }

    private async Task ViewCommandsAsync()
    {
        System.Console.WriteLine("\nLocal Commands:\n");

        var commands = await listCommands.HandleAsync(isGlobal: false);

        if (commands.Count == 0)
        {
            System.Console.WriteLine("  (No commands found)\n");
            return;
        }

        foreach (var command in commands)
        {
            System.Console.WriteLine($"  • {command.Name}");
            System.Console.WriteLine($"    {command.Description}");
            System.Console.WriteLine($"    Path: {command.FilePath}");
            System.Console.WriteLine();
        }

        System.Console.WriteLine($"Total: {commands.Count} command(s)\n");
    }

    private async Task ViewDiffAsync()
    {
        System.Console.WriteLine("\n🔍 Comparing Local vs Global Configuration...\n");

        var localRoot = Path.Combine(Directory.GetCurrentDirectory(), "local-config");
        var globalRoot = pathResolver.GetGlobalClaudePath();

        var diffs = await diffCalculator.CalculateDiffAsync(localRoot, globalRoot);

        var newFiles = diffs.Where(d => d.Status == DiffStatus.New).ToList();
        var modifiedFiles = diffs.Where(d => d.Status == DiffStatus.Modified).ToList();
        var deletedFiles = diffs.Where(d => d.Status == DiffStatus.Deleted).ToList();

        System.Console.WriteLine($"Summary:");
        System.Console.WriteLine($"  ➕ New:      {newFiles.Count}");
        System.Console.WriteLine($"  ✏️  Modified: {modifiedFiles.Count}");
        System.Console.WriteLine($"  ❌ Deleted:  {deletedFiles.Count}");
        System.Console.WriteLine();

        if (newFiles.Count > 0)
        {
            System.Console.WriteLine("New files (will be added):");
            foreach (var file in newFiles)
            {
                System.Console.WriteLine($"  ➕ {file.RelativePath}");
            }
            System.Console.WriteLine();
        }

        if (modifiedFiles.Count > 0)
        {
            System.Console.WriteLine("Modified files (will be updated):");
            foreach (var file in modifiedFiles)
            {
                System.Console.WriteLine($"  ✏️  {file.RelativePath}");
            }
            System.Console.WriteLine();
        }

        if (deletedFiles.Count > 0)
        {
            System.Console.WriteLine("Deleted files (exist in global but not local):");
            foreach (var file in deletedFiles)
            {
                System.Console.WriteLine($"  ❌ {file.RelativePath}");
            }
            System.Console.WriteLine();
        }

        if (newFiles.Count == 0 && modifiedFiles.Count == 0 && deletedFiles.Count == 0)
        {
            System.Console.WriteLine("✅ No differences found. Local and global are in sync.\n");
        }
    }

    private async Task ValidateAsync()
    {
        System.Console.WriteLine("\n🔍 Validating Configuration...\n");

        var localRoot = Path.Combine(Directory.GetCurrentDirectory(), "local-config");
        var result = await validator.ValidateAsync(localRoot);

        if (result.IsValid)
        {
            System.Console.WriteLine("✅ Configuration is valid!\n");
        }
        else
        {
            System.Console.WriteLine($"❌ Found {result.Errors.Count} validation error(s):\n");

            foreach (var error in result.Errors)
            {
                var icon = error.Severity == ValidationSeverity.Error ? "❌" : "⚠️";
                System.Console.WriteLine($"  {icon} {error.FilePath}");
                System.Console.WriteLine($"     {error.Message}");
                System.Console.WriteLine();
            }
        }
    }

    private async Task DeployDryRunAsync()
    {
        System.Console.WriteLine("\n🔍 Deploy Preview (Dry Run)\n");
        System.Console.WriteLine("Simulating deployment without making any changes...\n");

        var options = new DeployOptions(BackupExisting: false, DryRun: true);
        var result = await deployConfig.HandleAsync(options);

        System.Console.WriteLine("✅ Dry run complete!");
        System.Console.WriteLine($"   Files that would be deployed: {CountFilesToDeploy()}");
        System.Console.WriteLine();
        System.Console.WriteLine("No files were actually copied. This was a preview only.");
        System.Console.WriteLine("Use option 9 to perform the actual deployment.\n");

        int CountFilesToDeploy()
        {
            var localRoot = Path.Combine(Directory.GetCurrentDirectory(), "local-config");
            if (!Directory.Exists(localRoot))
                return 0;

            return Directory.GetFiles(localRoot, "*", SearchOption.AllDirectories).Length;
        }
    }

    private async Task DeployAsync()
    {
        System.Console.WriteLine("\n🚀 Deploy to ~/.claude\n");
        System.Console.WriteLine("This will copy all files from local-config/ to ~/.claude/");
        System.Console.WriteLine("A backup will be created automatically.");
        System.Console.WriteLine();
        System.Console.Write("Continue? (y/N): ");

        var confirm = System.Console.ReadLine()?.Trim().ToLowerInvariant();

        if (confirm != "y" && confirm != "yes")
        {
            System.Console.WriteLine("\nDeploy cancelled.\n");
            return;
        }

        System.Console.WriteLine("\n📦 Deploying...\n");

        var options = new DeployOptions(BackupExisting: true);
        var result = await deployConfig.HandleAsync(options);

        if (result.Success)
        {
            System.Console.WriteLine($"✅ Deploy successful!");
            System.Console.WriteLine($"   Files deployed: {result.FilesDeployed}");
            System.Console.WriteLine($"   Files backed up: {result.FilesBackedUp}");
            if (result.BackupPath is not null)
            {
                System.Console.WriteLine($"   Backup location: {result.BackupPath}");
            }
            System.Console.WriteLine();
        }
        else
        {
            System.Console.WriteLine($"❌ Deploy failed!");
            foreach (var error in result.Errors)
            {
                System.Console.WriteLine($"   Error: {error}");
            }
            System.Console.WriteLine();
        }
    }

    private async Task ImportAsync()
    {
        System.Console.WriteLine("\n⬇️  Import from ~/.claude\n");
        System.Console.WriteLine("This will copy files from ~/.claude/ to local-config/");
        System.Console.WriteLine("Your current local-config will be backed up.");
        System.Console.WriteLine();
        System.Console.Write("Continue? (y/N): ");

        var confirm = System.Console.ReadLine()?.Trim().ToLowerInvariant();

        if (confirm != "y" && confirm != "yes")
        {
            System.Console.WriteLine("\nImport cancelled.\n");
            return;
        }

        System.Console.WriteLine("\n📥 Importing...\n");

        var options = new ImportOptions(BackupLocal: true);
        var result = await importConfig.HandleAsync(options);

        if (result.Success)
        {
            System.Console.WriteLine($"✅ Import successful!");
            System.Console.WriteLine($"   Files imported: {result.FilesImported}");
            System.Console.WriteLine($"   Files backed up: {result.FilesBackedUp}");
            if (result.BackupPath is not null)
            {
                System.Console.WriteLine($"   Backup location: {result.BackupPath}");
            }
            System.Console.WriteLine();
        }
        else
        {
            System.Console.WriteLine($"❌ Import failed!");
            foreach (var error in result.Errors)
            {
                System.Console.WriteLine($"   Error: {error}");
            }
            System.Console.WriteLine();
        }
    }
}
