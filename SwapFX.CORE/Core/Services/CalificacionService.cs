using SwapFX.CORE.Core.Common;
using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Entities;
using SwapFX.CORE.Core.Interfaces;
namespace SwapFX.CORE.Core.Services;
public class CalificacionService : ICalificacionService
{
    private readonly ICalificacionRepository _calificaciones;
    public CalificacionService(ICalificacionRepository calificaciones) { _calificaciones = calificaciones; }

    public async Task<RespuestaApi<bool>> CrearAsync(int calificadorId, CrearCalificacionDTO dto)
    {
        if (dto.Puntuacion < 1 || dto.Puntuacion > 5)
            return RespuestaApi<bool>.Error("La puntuación debe ser entre 1 y 5.");
        var existe = await _calificaciones.ExisteCalificacionAsync(dto.TransaccionId, calificadorId);
        if (existe) return RespuestaApi<bool>.Error("Ya calificaste esta transacción.");
        await _calificaciones.AddAsync(new Calificacion {
            TransaccionId = dto.TransaccionId, UsuarioCalificadorId = calificadorId,
            UsuarioCalificadoId = dto.UsuarioCalificadoId, Puntuacion = dto.Puntuacion,
            Comentario = dto.Comentario, FechaCalificacion = DateTime.UtcNow
        });
        return RespuestaApi<bool>.Ok(true, "Calificación registrada.");
    }

    public async Task<RespuestaApi<IEnumerable<CalificacionListDTO>>> ListarPorUsuarioAsync(int usuarioId)
    {
        var lista = await _calificaciones.GetByUsuarioCalificadoAsync(usuarioId);
        return RespuestaApi<IEnumerable<CalificacionListDTO>>.Ok(lista.Select(c => new CalificacionListDTO {
            Id = c.Id, Puntuacion = c.Puntuacion, Comentario = c.Comentario,
            FechaCalificacion = c.FechaCalificacion,
            NombreCalificador = c.UsuarioCalificador?.Nombres + " " + c.UsuarioCalificador?.Apellidos
        }));
    }
}
