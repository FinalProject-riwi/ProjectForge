using ProjectForge.Core.Entities;
using ProjectForge.Core.Exceptions;
using Xunit;

namespace ProjectForge.Application.Tests;

public class ProjectDomainTests
{
    [Fact]
    public void Rename_trims_and_updates_the_name()
    {
        var project = Project.Create("Initial name", null, 1, 2);

        project.Rename("  Updated name  ");

        Assert.Equal("Updated name", project.Name);
    }

    [Fact]
    public void Rename_rejects_short_names()
    {
        var project = Project.Create("Initial name", null, 1, 2);

        var exception = Assert.Throws<DomainException>(() => project.Rename("ab"));

        Assert.Equal("El nombre del proyecto debe tener al menos 3 caracteres.", exception.Message);
        Assert.Equal("Initial name", project.Name);
    }

    [Fact]
    public void Create_rejects_non_positive_relationship_ids()
    {
        var exception = Assert.Throws<DomainException>(() => Project.Create("Project", null, 0, 1));

        Assert.Contains("userId", exception.Message);
    }
}
