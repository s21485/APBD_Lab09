using APBD_Lab09.Models.DTOs;

namespace APBD_Lab09.Services;

public interface IWarehouseService
{
    Task<ProductWarehouseResponse> AddProductToWarehouseAsync(ProductWarehouseRequest request);
}