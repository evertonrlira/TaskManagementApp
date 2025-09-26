namespace TaskManagement.Core.Interfaces;

/// <summary>
/// Generic pagination interface for query results
/// </summary>
/// <typeparam name="T">The type of items in the paginated list</typeparam>
public interface IPaginatedList<T>
{
	List<T> Items { get; }
	int PageNumber { get; }
	int PageSize { get; }
	int TotalCount { get; }
	int TotalPages { get; }
	bool HasPreviousPage { get; }
	bool HasNextPage { get; }
}

/// <summary>
/// Factory interface for creating paginated lists
/// </summary>
public interface IPaginationService
{
	Task<IPaginatedList<T>> CreateAsync<T>(IQueryable<T> source, int pageNumber, int pageSize);
}
