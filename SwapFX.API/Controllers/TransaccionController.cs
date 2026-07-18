using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Interfaces;
using System.Security.Claims;
namespace SwapFX.API.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TransaccionController : ControllerBase
{
    private readonly ITransaccionService _transacciones;
    public TransaccionController(ITransaccionService transacciones) { _transacciones = transacciones; }

    private int GetUsuarioId() => int.Parse(User.FindFirst("UsuarioId")!.Value);

    [HttpPost("iniciar")]
    public async Task<IActionResult> Iniciar([FromBody] IniciarTransaccionDTO dto)
    {
        var result = await _transacciones.IniciarAsync(GetUsuarioId(), dto);
        return result.Exito ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _transacciones.ListarPorUsuarioAsync(GetUsuarioId());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var result = await _transacciones.ObtenerDetalleAsync(id, GetUsuarioId());
        return result.Exito ? Ok(result) : NotFound(result);
    }

    [HttpPost("comprobante")]
    public async Task<IActionResult> ReportarComprobante([FromBody] ReportarComprobanteDTO dto)
    {
        try
        {
            var result = await _transacciones.ReportarComprobanteAsync(GetUsuarioId(), dto);
            return result.Exito ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpPut("{id}/confirmar")]
    public async Task<IActionResult> ConfirmarPago(int id)
    {
        var result = await _transacciones.ConfirmarPagoAsync(id, GetUsuarioId());
        return result.Exito ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id, [FromQuery] string motivo = "Sin motivo")
    {
        var result = await _transacciones.CancelarAsync(id, GetUsuarioId(), motivo);
        return result.Exito ? Ok(result) : BadRequest(result);
    }
}
