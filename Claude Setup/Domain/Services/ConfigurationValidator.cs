using Claude_Setup.Domain.Models;
using Claude_Setup.Infrastructure.FileSystem;

namespace Claude_Setup.Domain.Services;

public sealed class ConfigurationValidator(FrontmatterParser parser)
{
    public async Task<ValidationResult> ValidateAsync(string rootPath)
    {
        var errors = new List<ValidationError>();

        // Validate skills
        var skillsPath = Path.Combine(rootPath, "skills");
        if (Directory.Exists(skillsPath))
        {
            foreach (var skillDir in Directory.GetDirectories(skillsPath))
            {
                await ValidateSkillAsync(skillDir, errors);
            }
        }

        // Future: Validate agents, rules, commands

        return new ValidationResult(
            IsValid: errors.Count == 0,
            Errors: errors
        );
    }

    private async Task ValidateSkillAsync(string skillPath, List<ValidationError> errors)
    {
        var skillFilePath = Path.Combine(skillPath, "SKILL.md");

        if (!File.Exists(skillFilePath))
        {
            errors.Add(new ValidationError(
                FilePath: skillPath,
                Message: "SKILL.md file not found"
            ));
            return;
        }

        var content = await File.ReadAllTextAsync(skillFilePath);
        var (metadata, _) = parser.Parse(content);

        if (metadata.Count == 0)
        {
            errors.Add(new ValidationError(
                FilePath: skillFilePath,
                Message: "SKILL.md must have YAML frontmatter"
            ));
            return;
        }

        if (!metadata.ContainsKey("name"))
        {
            errors.Add(new ValidationError(
                FilePath: skillFilePath,
                Message: "SKILL.md frontmatter must contain 'name' field"
            ));
        }

        if (!metadata.ContainsKey("description"))
        {
            errors.Add(new ValidationError(
                FilePath: skillFilePath,
                Message: "SKILL.md frontmatter must contain 'description' field",
                Severity: ValidationSeverity.Warning
            ));
        }
    }
}
