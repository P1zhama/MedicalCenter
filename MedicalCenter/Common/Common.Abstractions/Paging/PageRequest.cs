namespace Common.Abstractions.Paging;

public static class PageRequest
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int page, int pageSize)
        => (page < 1 ? DefaultPage : page,
            pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize));

    public static int Skip(int page, int pageSize) => (page - 1) * pageSize;
}
