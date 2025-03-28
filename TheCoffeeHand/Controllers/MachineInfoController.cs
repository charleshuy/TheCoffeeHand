using Microsoft.AspNetCore.Mvc;
using Services.ServiceInterfaces;
using Domain.Entities;

namespace TheCoffeeHand.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class MachineInfoController: ControllerBase {
        private readonly IMachineInfoService _machineInfoService;

        public MachineInfoController(IMachineInfoService machineInfoService) {
            _machineInfoService = machineInfoService;
        }

        [HttpGet("machines")]
        public async Task<IActionResult> GetAllMachines() {
            var machines = await _machineInfoService.GetAllMachinesAsync();
            return Ok(machines);
        }

        [HttpGet("machines/{id}")]
        public async Task<IActionResult> GetMachineById(string id) {
            var machine = await _machineInfoService.GetMachineByIdAsync(id);
            if (machine == null)
                return NotFound();

            return Ok(machine);
        }

        [HttpGet("recipes")]
        public async Task<IActionResult> GetAllRecipes() {
            var recipes = await _machineInfoService.GetAllRecipesAsync();
            return Ok(recipes);
        }

        [HttpGet("recipes/{id}")]
        public async Task<IActionResult> GetRecipeById(string id) {
            var recipe = await _machineInfoService.GetRecipeByIdAsync(id);
            if (recipe == null)
                return NotFound();

            return Ok(recipe);
        }
    }
}
