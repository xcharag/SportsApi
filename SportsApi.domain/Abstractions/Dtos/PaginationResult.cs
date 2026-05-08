namespace SportsApi.domain.Abstractions.Dtos;

public class PaginationResult<TEntity> where TEntity : class
{
    public int Page { get; set; }
    public int PerPage { get; set; }
    public int TotalPages { get; set; }
    public int Count { get; set; }
    public IEnumerable<TEntity> Data { get; set; } = [];
}