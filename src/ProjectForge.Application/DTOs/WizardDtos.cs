using ProjectForge.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectForge.Application.DTOs;

// ─── Wizard Steps ─────────────────────────────────────────────────────────────

public class WizardStep1Dto
{
    [Required] public ArchitectureType Architecture { get; set; }
    public string? CustomDescription { get; set; }
}

public class WizardStep2Dto
{
    [Required] public FrameworkType Framework { get; set; }
    [Required] public string FrameworkVersion { get; set; } = string.Empty;
    [Required] public DatabaseType Database { get; set; }
}

public class WizardStep3Dto
{
    [Required] public InfrastructureType Infrastructure { get; set; }
    [Required] public DeploymentTarget DeploymentTarget { get; set; }
    public List<VpsCredentialDto> VpsCredentials { get; set; } = new();
}

public class WizardStep4Dto
{
    public List<string> SelectedPatterns { get; set; } = new();
    public List<string> SelectedLibraries { get; set; } = new();
}

public class WizardStep5Dto
{
    [Required, MinLength(3)] public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool CreatePrivateRepo { get; set; } = false;
}

// ─── Consolidado del wizard ───────────────────────────────────────────────────

public class CreateProjectDto
{
    public WizardStep1Dto Step1 { get; set; } = new();
    public WizardStep2Dto Step2 { get; set; } = new();
    public WizardStep3Dto Step3 { get; set; } = new();
    public WizardStep4Dto Step4 { get; set; } = new();
    public WizardStep5Dto Step5 { get; set; } = new();
}

// ─── VPS ──────────────────────────────────────────────────────────────────────

public class VpsCredentialDto
{
    [Required] public string Label { get; set; } = string.Empty;
    [Required] public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    [Required] public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? PrivateKeyPath { get; set; }
    public string Role { get; set; } = "worker";
}

// ─── Respuestas ───────────────────────────────────────────────────────────────

public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RepositoryUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Architecture { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
}

public class ProjectDetailDto : ProjectSummaryDto
{
    public string? GeneratedReadme { get; set; }
    public List<LogEntryDto> Logs { get; set; } = new();
    public WizardConfigDto Config { get; set; } = new();
}

public class WizardConfigDto
{
    public string Architecture { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string FrameworkVersion { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Infrastructure { get; set; } = string.Empty;
    public List<string> DesignPatterns { get; set; } = new();
    public List<string> Libraries { get; set; } = new();
}

public class LogEntryDto
{
    public string Step { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public string? Command { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AiSuggestionDto
{
    public List<string> SuggestedPatterns { get; set; } = new();
    public List<string> SuggestedLibraries { get; set; } = new();
    public string Rationale { get; set; } = string.Empty;
}

public class FrameworkOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<string> AvailableVersions { get; set; } = new();
}
