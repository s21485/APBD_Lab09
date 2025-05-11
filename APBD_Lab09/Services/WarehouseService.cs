using APBD_Lab09.Models.DTOs;
using Microsoft.Data.SqlClient;

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
        // Wartość ilości przekazana w żądaniu powinna być większa niż 0.
        if (request.Amount <= 0)
            throw new ArgumentException("Ilość musi być większa niż 0.");
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Sprawdzamy, czy produkt o podanym identyfikatorze istnieje.
                        using (SqlCommand command = new SqlCommand("SELECT 1 FROM Product WHERE IdProduct = @IdProduct",
                                   connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                            var productExists = await command.ExecuteScalarAsync();
                            if (productExists != null)
                            {
                                throw new KeyNotFoundException(
                                    $"Produkt o podanym ID ({request.IdProduct}) nie istnieje.");
                            }
                        }
                        // Następnie sprawdzamy, czy magazyn o podanym identyfikatorze istnieje. 
                        using (SqlCommand command = new SqlCommand("SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                            var warehouseExists = await command.ExecuteScalarAsync();
                            if (warehouseExists != null)
                                throw new KeyNotFoundException($"Magazyn {request.IdWarehouse} nie istnieje.");
                        }
                        // Dlatego sprawdzamy, czy w tabeli Order istnieje rekord z IdProduktu i Ilością (Amount), które odpowiadają naszemu żądaniu. 
                        // Data utworzenia zamówienia powinna być wcześniejsza niż data utworzenia w żądaniu
                        // Sprawdzamy, czy to zamówienie zostało przypadkiem zrealizowane.
                        // Aktualizujemy kolumnę FullfilledAt
                        // Wstawiamy rekord do tabeli Product_Warehouse. 
                        // zwracamy wartość klucza głównego wygenerowanego
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}