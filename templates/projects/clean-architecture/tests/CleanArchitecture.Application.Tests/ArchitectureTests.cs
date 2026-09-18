using CleanArchitecture.Domain;
using NetArchTest.Rules;
using Xunit;

namespace CleanArchitecture.Application.Tests;

/// <summary>
/// Enforces the Clean Architecture dependency rule automatically. See
/// skills/testing/architecture-testing.md -- these tests should stay in CI on every PR.
/// </summary>
public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Application_Or_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Product).Assembly)
            .Should()
            .NotHaveDependencyOnAny("CleanArchitecture.Application", "CleanArchitecture.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_EFCore()
    {
        var result = Types.InAssembly(typeof(Products.CreateProductHandler).Assembly)
            .Should()
            .NotHaveDependencyOnAny("CleanArchitecture.Infrastructure", "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
