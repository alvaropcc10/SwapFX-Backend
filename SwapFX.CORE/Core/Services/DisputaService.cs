using SwapFX.CORE.Core.Common;
using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Entities;
using SwapFX.CORE.Core.Interfaces;
namespace SwapFX.CORE.Core.Services;
public class DisputaService : IDisputaService
{
    private readonly IDisputaRepository _disputas;
    private readonly ITransaccionRepository _transacciones;
    public DisputaService(IDisputaRepository disputas, ITransaccionRepository transacciones)
    { _disputas = disputas; _transacciones = transacciones; }

    public async Task<RespuestaApi<bool>> CrearAsync(int usuarioId, CrearDisputaDTO dto)
    {
        var t = await _transacciones.GetByIdAsync(dto.TransaccionId);
        if (t == null) return RespuestaApi<bool>.Error("Transacción no encontrada.");
        if (t.CompradorId != usuarioId && t.VendedorId != usuarioId)
            return RespuestaApi<bool>.Error("No tienes acceso a esta transacción.");
        await _disputas.AddAsync(new Disputa {
            TransaccionId = dto.TransaccionId, UsuarioReportanteId = usuarioId,
            Motivo = dto.Motivo, Descripcion = dto.Descripcion,
            Estado = "ABIERTA", FechaReporte = DateTime.UtcNow
        });
        t.EstadoActual = Parametros.EstadosTransaccion.EnDisputa;
        await _transacciones.UpdateAsync(t);
        return RespuestaApi<bool>.Ok(true, "Disputa registrada.");
    }

    public async Task<RespuestaApi<IEnumerable<DisputaListDTO>>> ListarAsync()
    {
        var lista = await _disputas.GetAllAsync();
        return RespuestaApi<IEnumerable<DisputaListDTO>>.Ok(lista.Select(d => new DisputaListDTO {
            Id = d.Id, TransaccionId = d.TransaccionId, Motivo = d.Motivo,
            Descripcion = d.Descripcion, Estado = d.Estado, Resolucion = d.Resolucion,
            FechaReporte = d.FechaReporte
        }));
    }

    public async Task<RespuestaApi<bool>> ResolverAsync(int adminId, ResolverDisputaDTO dto)
    {
        var d = await _disputas.GetByIdAsync(dto.DisputaId);
        if (d == null) return RespuestaApi<bool>.Error("Disputa no encontrada.");
        d.Estado = "RESUELTA";
        d.Resolucion = dto.Resolucion;
        d.FechaResolucion = DateTime.UtcNow;
        d.AdminResolutorId = adminId;
        await _disputas.UpdateAsync(d);
        if (d.Transaccion != null) {
            d.Transaccion.EstadoActual = Parametros.EstadosTransaccion.Resuelta;
            await _transacciones.UpdateAsync(d.Transaccion);
        }
        return RespuestaApi<bool>.Ok(true, "Disputa resuelta.");
    }
}
