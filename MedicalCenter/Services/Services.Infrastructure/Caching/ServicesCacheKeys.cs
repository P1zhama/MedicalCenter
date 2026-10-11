namespace Services.Infrastructure.Caching;

internal static class ServicesCacheKeys
{
    public const string AllSpecializations = "specializations:all";

    public const string ActiveSpecializations = "specializations:active";

    public const string ServiceCatalog = "catalog";

    public const string AllServiceCategories = "service-categories:all";

    public static string Specialization(Guid id) => $"specialization:{id}";

    public static string SpecializationIsActive(Guid id) => $"specialization:{id}:active";

    public static string Service(Guid id) => $"service:{id}";
}
