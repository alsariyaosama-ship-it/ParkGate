namespace ParkGate.Domain.Entities;

/// <summary>
/// Age category (الفئة العمرية) for visitors and companions.
/// </summary>
public sealed class AgeCategory
{
    public const int MaxNameLength = 50;
    public const int MaxDescriptionLength = 100;

    public int Id { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string? DescriptionAr { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private AgeCategory() { }

    public AgeCategory(string nameAr, string? descriptionAr, int sortOrder, bool isActive = true)
    {
        ValidateName(nameAr);
        ValidateDescription(descriptionAr);

        NameAr = nameAr.Trim();
        DescriptionAr = string.IsNullOrWhiteSpace(descriptionAr) ? null : descriptionAr.Trim();
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    public void Update(string nameAr, string? descriptionAr, int sortOrder)
    {
        ValidateName(nameAr);
        ValidateDescription(descriptionAr);

        NameAr = nameAr.Trim();
        DescriptionAr = string.IsNullOrWhiteSpace(descriptionAr) ? null : descriptionAr.Trim();
        SortOrder = sortOrder;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void ValidateName(string nameAr)
    {
        if (string.IsNullOrWhiteSpace(nameAr) || nameAr.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException($"Age category name must be between 1 and {MaxNameLength} characters.", nameof(nameAr));
        }
    }

    private static void ValidateDescription(string? descriptionAr)
    {
        if (descriptionAr != null && descriptionAr.Trim().Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Age category description cannot exceed {MaxDescriptionLength} characters.", nameof(descriptionAr));
        }
    }
}
