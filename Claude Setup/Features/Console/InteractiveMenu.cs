using Claude_Setup.Features.Skills;

namespace Claude_Setup.Features.Console;

public sealed class InteractiveMenu(ListSkills listSkills)
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
        System.Console.WriteLine("  1. View Skills (Local)");
        System.Console.WriteLine("  2. View Skills (Global ~/.claude)");
        System.Console.WriteLine("  3. View Agents (Local)");
        System.Console.WriteLine("  4. View Rules (Local)");
        System.Console.WriteLine("  5. View Commands (Local)");
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
                System.Console.WriteLine("\n(Agents view not yet implemented)\n");
                break;

            case "4":
                System.Console.WriteLine("\n(Rules view not yet implemented)\n");
                break;

            case "5":
                System.Console.WriteLine("\n(Commands view not yet implemented)\n");
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
}
