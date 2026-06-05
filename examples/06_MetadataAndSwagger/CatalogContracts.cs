using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Annotations;

namespace InfiniteRefactor.Infrastructure.Examples.MetadataAndSwagger;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[DataService(Name = "catalog", Description = "Sample product catalog")]
public interface ICatalogService
{
    [DataServiceMethod(Name = "get")]
    Task<ProductDto> GetProductAsync(int id);
}

public class CatalogService : ICatalogService
{
    public Task<ProductDto> GetProductAsync(int id) =>
        Task.FromResult(new ProductDto { Id = id, Name = $"Product-{id}" });
}
