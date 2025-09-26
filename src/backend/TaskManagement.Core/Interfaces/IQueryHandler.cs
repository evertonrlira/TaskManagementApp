namespace TaskManagement.Core.Interfaces;

/// <summary>
/// Base interface for all queries
/// </summary>
public interface IQuery
{
}

/// <summary>
/// Base interface for all query handlers
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResult">The result type</typeparam>
public interface IQueryHandler<in TQuery, TResult>
	where TQuery : IQuery
{
	Task<TResult> HandleAsync(TQuery query);
}
