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

                        DateTime createdAtDate;
                        if (!DateTime.TryParse(request.CreatedAt, out createdAtDate))
                        {
                            throw new FormatException("Nieprawidłowy format daty");
                        }
                        int orderId;
                        using (SqlCommand command = new SqlCommand(
                                   @"SELECT IdOrder 
                                FROM [Order] 
                                WHERE IdProduct = @IdProduct 
                                AND Amount = @Amount 
                                AND CreatedAt < @CreatedAt 
                                AND FulfilledAt IS NULL", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                            command.Parameters.AddWithValue("@Amount", request.Amount);
                            command.Parameters.AddWithValue("@CreatedAt", createdAtDate);
                                
                            var result = await command.ExecuteScalarAsync();
                            if (result == null)
                            {
                                throw new InvalidOperationException("Nie znaleziono pasującego, niezrealizowanego zamówienia");
                            }
                            orderId = Convert.ToInt32(result);
                        }
                        // Sprawdzamy, czy w tabeli Order istnieje rekord z IdProduktu i Ilością (Amount), które odpowiadają naszemu żądaniu. 
                        // Data utworzenia zamówienia powinna być wcześniejsza niż data utworzenia w żądaniu
                        
                        // Sprawdzamy, czy to zamówienie zostało przypadkiem zrealizowane.
                        using (SqlCommand command = new SqlCommand(
                                   "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdOrder", orderId);
                            var alreadyFulfilled = await command.ExecuteScalarAsync();
                            if (alreadyFulfilled != null)
                            {
                                throw new InvalidOperationException("To zamówienie zostało już zrealizowane");
                            }
                        }
                        
                        // Aktualizujemy kolumnę FullfilledAt
                        using (SqlCommand command = new SqlCommand(
                                   "UPDATE [Order] SET FulfilledAt = @Now WHERE IdOrder = @IdOrder", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Now", DateTime.Now);
                            command.Parameters.AddWithValue("@IdOrder", orderId);
                            await command.ExecuteNonQueryAsync();
                        }
                        
                        // Pobranie ceny
                        decimal productPrice;
                        using (SqlCommand command = new SqlCommand(
                                   "SELECT Price FROM Product WHERE IdProduct = @IdProduct", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                            productPrice = Convert.ToDecimal(await command.ExecuteScalarAsync());
                        }                        
                        
                        // Wstawiamy rekord do tabeli Product_Warehouse. 
                        int idProductWarehouse;

                        using (SqlCommand command = new SqlCommand(
                                   @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                                VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @Now);
                                SELECT SCOPE_IDENTITY()", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                            command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                            command.Parameters.AddWithValue("@IdOrder", orderId);
                            command.Parameters.AddWithValue("@Amount", request.Amount);
                            command.Parameters.AddWithValue("@Price", productPrice * request.Amount);
                            command.Parameters.AddWithValue("@Now", DateTime.Now);
                                
                            idProductWarehouse = Convert.ToInt32(await command.ExecuteScalarAsync());
                        }
                        
                        // zwracamy wartość klucza głównego wygenerowanego
                        transaction.Commit();
                        return new ProductWarehouseResponse { IdProductWarehouse = idProductWarehouse };
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