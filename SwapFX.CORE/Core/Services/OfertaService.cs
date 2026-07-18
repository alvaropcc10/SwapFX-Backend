using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Entities;
using SwapFX.CORE.Core.Interfaces;
namespace SwapFX.CORE.Core.Services;
public class OfertaService : IOfertaService
{
    private readonly IOfertaRepository _ofertaRepository;
    public OfertaService(IOfertaRepository ofertaRepository)
    {
        _ofertaRepository = ofertaRepository;
    }
    public async Task<IEnumerable<OfertaListDTO>> GetOfertas(string? tipo, int? monedaOrigenId, int? monedaDestinoId)
    {
        var ofertas = await _ofertaRepository.GetOfertas(tipo, monedaOrigenId, monedaDestinoId);
        return ofertas.Select(o => new OfertaListDTO {
            Id = o.Id, Tipo = o.Tipo, MonedaOrigen = o.MonedaOrigen?.Codigo,
            MonedaDestino = o.MonedaDestino?.Codigo, Monto = o.Monto, TipoCambio = o.TipoCambio,
            Estado = o.Estado, Notas = o.Notas, FechaPublicacion = o.FechaPublicacion,
            FechaExpiracion = o.FechaExpiracion,
            UsuarioNombre = o.Usuario?.Nombres + " " + o.Usuario?.Apellidos,
            UsuarioId = o.UsuarioId
        });
    }
    public async Task<OfertaListDTO?> GetOfertaById(int id)
    {
        var o = await _ofertaRepository.GetOfertaById(id);
        if (o == null) return null;
        return new OfertaListDTO {
            Id = o.Id, Tipo = o.Tipo, MonedaOrigen = o.MonedaOrigen?.Codigo,
            MonedaDestino = o.MonedaDestino?.Codigo, Monto = o.Monto, TipoCambio = o.TipoCambio,
            Estado = o.Estado, Notas = o.Notas, FechaPublicacion = o.FechaPublicacion,
            FechaExpiracion = o.FechaExpiracion,
            UsuarioNombre = o.Usuario?.Nombres + " " + o.Usuario?.Apellidos,
            UsuarioId = o.UsuarioId
        };
    }
    public async Task<int> CreateOferta(CreateOfertaDTO dto, int usuarioId)
    {
        var oferta = new Oferta {
            UsuarioId = usuarioId, Tipo = dto.Tipo, MonedaOrigenId = dto.MonedaOrigenId,
            MonedaDestinoId = dto.MonedaDestinoId, Monto = dto.Monto, TipoCambio = dto.TipoCambio,
            FechaPublicacion = DateTime.UtcNow, FechaExpiracion = DateTime.UtcNow.AddHours(dto.ValidezHoras),
            Estado = "PUBLICADA", Notas = dto.Notas, IsActive = true
        };
        return await _ofertaRepository.CreateOferta(oferta);
    }
    public async Task UpdateOferta(UpdateOfertaDTO dto)
    {
        var oferta = await _ofertaRepository.GetOfertaById(dto.Id);
        if (oferta == null) return;
        oferta.Monto = dto.Monto;
        oferta.TipoCambio = dto.TipoCambio;
        oferta.Notas = dto.Notas;
        await _ofertaRepository.UpdateOferta(oferta);
    }
    public async Task DeleteOferta(int id)
    {
        await _ofertaRepository.DeleteOferta(id);
    }
}
