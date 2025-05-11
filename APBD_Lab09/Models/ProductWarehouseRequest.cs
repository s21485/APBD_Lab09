namespace APBD_Lab09.Models;

public class ProductWarehouseRequest
{
    public int IdProduct { get; set; }
    public int IdWarehouse { get; set; }
    public int Amount { get; set; }
    public string CreatedAt { get; set; }
}