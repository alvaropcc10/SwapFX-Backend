using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Interfaces;
namespace SwapFX.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OfertaController : ControllerBase
{
    private readonly IOfertaService _ofertaService;
    public OfertaController(IOfertaService ofertaService)
    {
        _ofertaService = ofertaService;
    }
    [HttpGet]
    public async Task<IActionResult> GetOfertas([FromQuery] string? tipo, [FromQuery] int? monedaOrigenId, [FromQuery] int? monedaDestinoId)
    {
        var ofertas = await _ofertaService.GetOfertas(tipo, monedaOrigenId, monedaDestinoId);
        return Ok(ofertas);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOfertaById(int id)
    {
        var oferta = await _ofertaService.GetOfertaById(id);
        if (oferta == null) return NotFound();
        return Ok(oferta);
    }
    [HttpPost]
    public async Task<IActionResult> CreateOferta([FromBody] CreateOfertaDTO dto)
    {
        try
        {
            var usuarioIdClaim = User.FindFirst("UsuarioId")?.Value;
            if (usuarioIdClaim == null) return Unauthorized();
            var usuarioId = int.Parse(usuarioIdClaim);
            var id = await _ofertaService.CreateOferta(dto, usuarioId);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }
    [HttpPut]
    public async Task<IActionResult> UpdateOferta([FromBody] UpdateOfertaDTO dto)
    {
        var existing = await _ofertaService.GetOfertaById(dto.Id);
        if (existing == null) return NotFound();
        await _ofertaService.UpdateOferta(dto);
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOferta(int id)
    {
        var existing = await _ofertaService.GetOfertaById(id);
        if (existing == null) return NotFound();
        await _ofertaService.DeleteOferta(id);
        return NoContent();
    }
}
