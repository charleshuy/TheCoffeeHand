using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/drink")]
    [ApiController]
    public class DrinkController : ControllerBase
    {
        private readonly IDrinkService _drinkService;

        public DrinkController(IDrinkService drinkService)
        {
            _drinkService = drinkService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDrink([FromBody] DrinkRequestDTO drinkDTO)
        {
            var result = await _drinkService.CreateDrinkAsync(drinkDTO);
            return CreatedAtAction(nameof(GetDrinkById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDrinkById(Guid id)
        {
            var drink = await _drinkService.GetDrinkByIdAsync(id);
            if (drink == null)
                return NotFound(new { message = "Drink not found" });
            return Ok(drink);
        }

        [HttpGet]
        public async Task<IActionResult> GetDrinks()
        {
            var drinks = await _drinkService.GetDrinksAsync();
            return Ok(drinks);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetDrinksPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var drinks = await _drinkService.GetDrinksAsync(pageNumber, pageSize);
            return Ok(drinks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDrink(Guid id, [FromBody] DrinkRequestDTO drinkDTO)
        {
            var updatedDrink = await _drinkService.UpdateDrinkAsync(id, drinkDTO);
            return Ok(updatedDrink);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDrink(Guid id)
        {
            await _drinkService.DeleteDrinkAsync(id);
            return NoContent();
        }
    }
}
