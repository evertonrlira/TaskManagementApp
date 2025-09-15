using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Infrastructure.Common;

public record PaginatedList<T>(
	List<T> Items,
	int PageNumber,
	int PageSize,
	int TotalCount,
	int TotalPages) : IPaginatedList<T>
{
	public bool HasPreviousPage => PageNumber > 1;
	public bool HasNextPage => PageNumber < TotalPages;

	public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
	{
		var totalCount = await source.CountAsync();
		var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
		pageNumber = Math.Clamp(pageNumber, 1, Math.Max(1, totalPages));

		var items = await source
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return new PaginatedList<T>(items, pageNumber, pageSize, totalCount, totalPages);
	}
}

public class PaginationService : IPaginationService
{
	public async Task<IPaginatedList<T>> CreateAsync<T>(IQueryable<T> source, int pageNumber, int pageSize)
	{
		return await PaginatedList<T>.CreateAsync(source, pageNumber, pageSize);
	}
}
