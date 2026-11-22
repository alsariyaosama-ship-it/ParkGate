namespace ParkGate.Domain.Entities;

/// <summary>
/// City (المدينة) a visitor comes from. A fixed seeded list: there is no management screen, so changes go
/// through a migration (BR-32).
/// </summary>
public sealed class City
{
    public const int MaxNameLength = 50;

    public int Id { get; private set; }
    public string NameAr { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private City() { }

    public City(string nameAr, int sortOrder, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(nameAr) || nameAr.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException($"City name must be between 1 and {MaxNameLength} characters.", nameof(nameAr));
        }

        NameAr = nameAr.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}
