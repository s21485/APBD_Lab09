using APBD_Lab09.Models.DTOs;
using APBD_Lab09.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Lab09.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController: ControllerBase
    {
        private IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToWarehouse(ProductWarehouseRequest request)
        {
            try
            {
                var result = await _warehouseService.AddProductToWarehouseAsync(request);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}