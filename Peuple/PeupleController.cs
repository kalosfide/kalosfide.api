using KalosfideAPI.Partages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{

    [Route("api/[controller]")]
    [ApiController]
    public class PeupleController: BaseController
    {

        private readonly IPeupleService _peupleService;

        public PeupleController(IPeupleService peupleService) : base()
        {
            _peupleService = peupleService;
        }

        [HttpGet("/api/peuple/estPeuple")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> EstPeuplé()
        {
            return Ok(await _peupleService.EstPeuplé());
        }

        [HttpPost("/api/peuple/peuple")]
        [ProducesResponseType(200)] // Ok
        [ProducesResponseType(404)] // Not found
        [AllowAnonymous]
        public async Task<IActionResult> Peuple()
        {
            if (await _peupleService.EstPeuplé())
            {
                return BadRequest(); 
            }

            RetourDeService retour = await _peupleService.Peuple();
            return SaveChangesActionResult(retour);
        }
    }
}
