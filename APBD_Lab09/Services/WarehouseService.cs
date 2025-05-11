using APBD_Lab09.Models.DTOs;

namespace APBD_Lab09.Services;

public class WarehouseService : IWarehouseService
{
    private string _connectionString;

    public WarehouseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<ProductWarehouseResponse> AddProductToWarehouseAsync(ProductWarehouseRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.Amount <= 0)
            throw new ArgumentException("Ilość musi być większa niż 0.");
    }
}