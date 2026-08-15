using System.Collections.Generic;

namespace Gateway.Api.Models;

public record NewServiceItemWebRequest(
    string Name,
    decimal Price,
    string CategoryId,
    string Status
);

public record CreateSpecializationWebRequest(
    string Name,
    string Status,
    IReadOnlyList<NewServiceItemWebRequest> Services
);

public record UpdateSpecializationWebRequest(
    string Name,
    string Status
);

public record ChangeStatusWebRequest(
    string Status
);

public record CreateServiceWebRequest(
    string Name,
    decimal Price,
    string SpecializationId,
    string CategoryId,
    string Status
);

public record UpdateServiceWebRequest(
    string Name,
    decimal Price,
    string CategoryId,
    string Status
);

public record CreatedSpecializationWebResponse(string SpecializationId);

public record CreatedServiceWebResponse(string ServiceId);

public record SpecializationListItemWebResponse(
    string Id,
    string Name,
    string Status
);

public record PublicSpecializationWebResponse(
    string Id,
    string Name
);

public record ServiceListItemWebResponse(
    string Id,
    string Name,
    decimal Price,
    string Status,
    string CategoryId,
    string CategoryName
);

public record SpecializationWebResponse(
    string Id,
    string Name,
    string Status,
    IReadOnlyList<ServiceListItemWebResponse> Services
);

public record ServiceWebResponse(
    string Id,
    string Name,
    decimal Price,
    string Status,
    string CategoryId,
    string CategoryName,
    string SpecializationId,
    string SpecializationName
);

public record CatalogServiceWebResponse(
    string Id,
    string Name,
    decimal Price
);

public record CatalogSpecializationWebResponse(
    string Id,
    string Name,
    IReadOnlyList<CatalogServiceWebResponse> Services
);

public record CatalogCategoryWebResponse(
    string Id,
    string Name,
    IReadOnlyList<CatalogSpecializationWebResponse> Specializations
);

public record ServiceCatalogWebResponse(
    IReadOnlyList<CatalogCategoryWebResponse> Categories
);
