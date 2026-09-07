using System.Collections.Generic;

namespace Gateway.Api.Models;

public record PagedWebResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);
