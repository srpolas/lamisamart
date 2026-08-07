namespace LamisaMart.Catalog.Application.DTOs;

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsFeatured { get; init; }
    public List<CategoryDto> SubCategories { get; init; } = new();
}
