namespace Services.Application.Common.Dtos;

public record CatalogServiceDto(
    Guid Id,
    string Name,
    decimal Price);

public record CatalogSpecializationDto(
    Guid Id,
    string Name,
    IReadOnlyList<CatalogServiceDto> Services);

public record CatalogCategoryDto(
    Guid Id,
    string Name,
    IReadOnlyList<CatalogSpecializationDto> Specializations);

public record ServiceCatalogDto(
    IReadOnlyList<CatalogCategoryDto> Categories);
