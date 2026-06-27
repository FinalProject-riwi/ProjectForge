using Microsoft.EntityFrameworkCore;
using ProjectForge.Core.Entities;
using ProjectForge.Infrastructure.Seeders.DotNet;
using ProjectForge.Infrastructure.Seeders.JavaScript;
using ProjectForge.Infrastructure.Seeders.Python;

namespace ProjectForge.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WizardConfig> WizardConfigs => Set<WizardConfig>();
    public DbSet<ProjectTemplate> Templates => Set<ProjectTemplate>();
    public DbSet<LibraryRecommendation> Libraries => Set<LibraryRecommendation>();
    public DbSet<DesignPatternEntry> DesignPatterns => Set<DesignPatternEntry>();
    public DbSet<ProjectLog> ProjectLogs => Set<ProjectLog>();
    public DbSet<VpsCredential> VpsCredentials => Set<VpsCredential>();
    public DbSet<AiSuggestionCache> AiSuggestionCaches => Set<AiSuggestionCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser
        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.HasIndex(u => u.GitHubId).IsUnique();
            e.Property(u => u.AccessToken).HasMaxLength(512);
        });

        // Project
        modelBuilder.Entity<Project>(e =>
        {
            e.HasOne(p => p.User).WithMany(u => u.Projects).HasForeignKey(p => p.UserId);
            e.HasOne(p => p.WizardConfig).WithOne().HasForeignKey<Project>(p => p.WizardConfigId);
            e.HasMany(p => p.Logs).WithOne(l => l.Project).HasForeignKey(l => l.ProjectId);
            e.Property(p => p.Status).HasConversion<string>();
        });

        // WizardConfig
        modelBuilder.Entity<WizardConfig>(e =>
        {
            e.Property(w => w.Architecture).HasConversion(ArchitectureTypeValueConverter.Instance);
            e.Property(w => w.Framework).HasConversion<string>();
            e.Property(w => w.Database).HasConversion<string>();
            e.Property(w => w.Infrastructure).HasConversion<string>();
            e.Property(w => w.DeploymentTarget).HasConversion<string>();
            e.HasMany(w => w.VpsCredentials).WithOne(v => v.WizardConfig).HasForeignKey(v => v.WizardConfigId);
        });

        // ProjectTemplate
        modelBuilder.Entity<ProjectTemplate>(e =>
        {
            e.Property(t => t.Architecture).HasConversion(ArchitectureTypeValueConverter.Instance);
            e.Property(t => t.Framework).HasConversion<string>();
            e.Property(t => t.Database).HasConversion<string>();
            e.Property(t => t.Infrastructure).HasConversion<string>();
            e.HasIndex(t => new { t.Architecture, t.TemplateType, t.Database, t.Infrastructure });
        });

        // LibraryRecommendation — mantener la navegación SuggestedWithPatterns
        // para que EF genere la misma shadow property LibraryRecommendationId que ya está en la migración
        modelBuilder.Entity<LibraryRecommendation>(e =>
        {
            e.Property(l => l.Architecture).HasConversion(ArchitectureTypeValueConverter.Instance);
            e.Property(l => l.Framework).HasConversion<string>();
        });

        // DesignPatternEntry — tiene FK opcional a LibraryRecommendation (shadow property)
        modelBuilder.Entity<DesignPatternEntry>(e =>
        {
            e.Property(d => d.Pattern).HasConversion<string>();
            e.Property(d => d.Architecture).HasConversion(ArchitectureTypeValueConverter.Instance);
        });

        // VpsCredential
        modelBuilder.Entity<VpsCredential>(e =>
        {
            e.Property(v => v.EncryptedPassword).HasMaxLength(1024);
        });

        // AiSuggestionCache
        modelBuilder.Entity<AiSuggestionCache>(e =>
        {
            e.HasIndex(c => c.CacheKey).IsUnique();
        });

        DotNetSeeder.Seed(modelBuilder);
        PythonSeeder.Seed(modelBuilder);
        JavaScriptSeeder.Seed(modelBuilder);
    }
}
