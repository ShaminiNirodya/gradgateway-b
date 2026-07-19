using GradGateway.Business.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GradGateway.Business.Helpers;

public static class QueryablePagingExtensions
{
    public static async Task<PagedResultDto<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize) = Pagination.Normalize(page, pageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<T>(items, total, normalizedPage, normalizedPageSize);
    }
}
