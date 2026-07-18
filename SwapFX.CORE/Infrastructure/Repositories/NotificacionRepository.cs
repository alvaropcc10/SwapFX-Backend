using Microsoft.EntityFrameworkCore;
using SwapFX.CORE.Core.Entities;
using SwapFX.CORE.Core.Interfaces;
using SwapFX.CORE.Infrastructure.Data;
namespace SwapFX.CORE.Infrastructure.Repositories;
public class NotificacionRepository : INotificacionRepository
{
    private readonly SwapFXDbContext _context;
    public NotificacionRepository(SwapFXDbContext context) { _context = context; }

    public async Task<IEnumerable<Notificacion>> GetByUsuarioAsync(int usuarioId)
        => await _context.Notificacion
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync();

    public async Task MarcarLeidaAsync(int notificacionId)
    {
        var n = await _context.Notificacion.FindAsync(notificacionId);
        if (n != null) { n.Leida = true; await _context.SaveChangesAsync(); }
    }

    public async Task CrearAsync(int usuarioId, string tipo, string mensaje)
    {
        _context.Notificacion.Add(new Notificacion {
            UsuarioId = usuarioId, Tipo = tipo, Mensaje = mensaje,
            Leida = false, FechaCreacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
