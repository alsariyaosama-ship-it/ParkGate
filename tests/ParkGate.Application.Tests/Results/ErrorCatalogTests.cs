using System.Reflection;
using ParkGate.Application.Common;

namespace ParkGate.Application.Tests.Results;

public sealed class ErrorCatalogTests
{
    [Fact]
    public void AllErrorsInCatalog_ShouldHaveNonEmptyAndUniqueCodes()
    {
        var nestedClasses = typeof(Errors).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var nested in nestedClasses)
        {
            var fields = nested.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Error));

            foreach (var field in fields)
            {
                var error = (Error?)field.GetValue(null);
                error.ShouldNotBeNull();
                error.Code.ShouldNotBeNullOrWhiteSpace($"Field {nested.Name}.{field.Name} has empty code");
                error.Message.ShouldNotBeNullOrWhiteSpace($"Field {nested.Name}.{field.Name} has empty message");

                seenCodes.Add(error.Code).ShouldBeTrue($"Duplicate error code found: {error.Code} in {nested.Name}.{field.Name}");
            }
        }
    }
}
