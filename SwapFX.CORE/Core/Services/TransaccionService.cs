using SwapFX.CORE.Core.Common;
using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Entities;
using SwapFX.CORE.Core.Interfaces;
namespace SwapFX.CORE.Core.Services;
public class TransaccionService : ITransaccionService
{
    private readonly ITransaccionRepository _transacciones;
    private readonly IOfertaRepository _ofertas;
    private readonly INotificacionRepository _notificaciones;

    public TransaccionService(ITransaccionRepository transacciones, IOfertaRepository ofertas, INotificacionRepository notificaciones)
    {
        _transacciones = transacciones;
        _ofertas = ofertas;
        _notificaciones = notificaciones;
    }

    public async Task<RespuestaApi<TransaccionDetalleDTO>> IniciarAsync(int usuarioId, IniciarTransaccionDTO dto)
    {
        var oferta = await _ofertas.GetOfertaById(dto.OfertaId);
        if (oferta == null) return RespuestaApi<TransaccionDetalleDTO>.Error("Oferta no encontrada.");
        if (oferta.UsuarioId == usuarioId) return RespuestaApi<TransaccionDetalleDTO>.Error("No puedes iniciar una transacción con tu propia oferta.");
        if (oferta.Estado != Parametros.EstadosOferta.Publicada) return RespuestaApi<TransaccionDetalleDTO>.Error("La oferta no está disponible.");

        var compradorId = oferta.Tipo == "V" ? usuarioId : oferta.UsuarioId!.Value;
        var vendedorId = oferta.Tipo == "V" ? oferta.UsuarioId!.Value : usuarioId;

        var transaccion = new Transaccion {
            Codigo = $"TX-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            OfertaCompraId = dto.OfertaId,
            OfertaVentaId = dto.OfertaId,
            CompradorId = compradorId,
            VendedorId = vendedorId,
            MontoOrigen = oferta.Monto,
            MontoDestino = oferta.Monto,
            TipoCambioAplicado = oferta.TipoCambio,
            EstadoActual = Parametros.EstadosTransaccion.Iniciada,
            FechaInicio = DateTime.UtcNow,
            IsActive = true
        };

        var creada = await _transacciones.AddAsync(transaccion);
        await _transacciones.AddBitacoraAsync(new BitacoraTransaccion {
            TransaccionId = creada.Id,
            EstadoAnterior = "",
            EstadoNuevo = Parametros.EstadosTransaccion.Iniciada,
            UsuarioResponsableId = usuarioId,
            FechaCambio = DateTime.UtcNow
        });

        await _notificaciones.CrearAsync(vendedorId, "TRANSACCION", $"Nueva transacción {creada.Codigo} iniciada.");
        await _notificaciones.CrearAsync(compradorId, "TRANSACCION", $"Transacción {creada.Codigo} iniciada correctamente.");

        var detalle = await _transacciones.GetByIdAsync(creada.Id);
        return RespuestaApi<TransaccionDetalleDTO>.Ok(MapearDetalle(detalle!), "Transacción iniciada.");
    }

    public async Task<RespuestaApi<IEnumerable<TransaccionListDTO>>> ListarPorUsuarioAsync(int usuarioId)
    {
        var lista = await _transacciones.GetByUsuarioAsync(usuarioId);
        return RespuestaApi<IEnumerable<TransaccionListDTO>>.Ok(lista.Select(MapearLista));
    }

    public async Task<RespuestaApi<TransaccionDetalleDTO>> ObtenerDetalleAsync(int transaccionId, int usuarioId)
    {
        var t = await _transacciones.GetByIdAsync(transaccionId);
        if (t == null) return RespuestaApi<TransaccionDetalleDTO>.Error("Transacción no encontrada.");
        if (t.CompradorId != usuarioId && t.VendedorId != usuarioId)
            return RespuestaApi<TransaccionDetalleDTO>.Error("No tienes acceso a esta transacción.");
        return RespuestaApi<TransaccionDetalleDTO>.Ok(MapearDetalle(t));
    }

    public async Task<RespuestaApi<bool>> ReportarComprobanteAsync(int usuarioId, ReportarComprobanteDTO dto)
    {
        var t = await _transacciones.GetByIdAsync(dto.TransaccionId);
        if (t == null) return RespuestaApi<bool>.Error("Transacción no encontrada.");
        if (t.CompradorId != usuarioId) return RespuestaApi<bool>.Error("Solo el comprador puede reportar el comprobante.");

        await _transacciones.AddComprobanteAsync(new Comprobante {
            TransaccionId = dto.TransaccionId,
            UsuarioId = usuarioId,
            NombreArchivo = dto.NombreArchivo,
            RutaArchivo = dto.RutaArchivo,
            FormatoArchivo = dto.FormatoArchivo,
            NumeroOperacion = dto.NumeroOperacion,
            FechaTransferencia = dto.FechaTransferencia,
            FechaSubida = DateTime.UtcNow
        });

        var estadoAnterior = t.EstadoActual!;
        t.EstadoActual = Parametros.EstadosTransaccion.PagoReportado;
        await _transacciones.UpdateAsync(t);
        await _transacciones.AddBitacoraAsync(new BitacoraTransaccion {
            TransaccionId = t.Id, EstadoAnterior = estadoAnterior,
            EstadoNuevo = Parametros.EstadosTransaccion.PagoReportado,
            UsuarioResponsableId = usuarioId, FechaCambio = DateTime.UtcNow
        });
        await _notificaciones.CrearAsync(t.VendedorId!.Value, "PAGO", $"El comprador reportó el pago en la transacción {t.Codigo}.");
        return RespuestaApi<bool>.Ok(true, "Comprobante reportado.");
    }

    public async Task<RespuestaApi<bool>> ConfirmarPagoAsync(int transaccionId, int usuarioId)
    {
        var t = await _transacciones.GetByIdAsync(transaccionId);
        if (t == null) return RespuestaApi<bool>.Error("Transacción no encontrada.");
        if (t.VendedorId != usuarioId) return RespuestaApi<bool>.Error("Solo el vendedor puede confirmar el pago.");
        if (t.EstadoActual != Parametros.EstadosTransaccion.PagoReportado)
            return RespuestaApi<bool>.Error("La transacción no está en estado de pago reportado.");

        var estadoAnterior = t.EstadoActual;
        t.EstadoActual = Parametros.EstadosTransaccion.Finalizada;
        t.FechaFinalizacion = DateTime.UtcNow;
        await _transacciones.UpdateAsync(t);
        await _transacciones.AddBitacoraAsync(new BitacoraTransaccion {
            TransaccionId = t.Id, EstadoAnterior = estadoAnterior,
            EstadoNuevo = Parametros.EstadosTransaccion.Finalizada,
            UsuarioResponsableId = usuarioId, FechaCambio = DateTime.UtcNow
        });
        await _notificaciones.CrearAsync(t.CompradorId!.Value, "FINALIZADA", $"Transacción {t.Codigo} finalizada exitosamente.");
        return RespuestaApi<bool>.Ok(true, "Pago confirmado. Transacción finalizada.");
    }

    public async Task<RespuestaApi<bool>> CancelarAsync(int transaccionId, int usuarioId, string motivo)
    {
        var t = await _transacciones.GetByIdAsync(transaccionId);
        if (t == null) return RespuestaApi<bool>.Error("Transacción no encontrada.");
        if (t.CompradorId != usuarioId && t.VendedorId != usuarioId)
            return RespuestaApi<bool>.Error("No tienes acceso a esta transacción.");

        var estadoAnterior = t.EstadoActual!;
        t.EstadoActual = Parametros.EstadosTransaccion.Cancelada;
        await _transacciones.UpdateAsync(t);
        await _transacciones.AddBitacoraAsync(new BitacoraTransaccion {
            TransaccionId = t.Id, EstadoAnterior = estadoAnterior,
            EstadoNuevo = Parametros.EstadosTransaccion.Cancelada,
            UsuarioResponsableId = usuarioId, Comentario = motivo, FechaCambio = DateTime.UtcNow
        });
        var otroUsuario = t.CompradorId == usuarioId ? t.VendedorId!.Value : t.CompradorId!.Value;
        await _notificaciones.CrearAsync(otroUsuario, "CANCELADA", $"La transacción {t.Codigo} fue cancelada.");
        return RespuestaApi<bool>.Ok(true, "Transacción cancelada.");
    }

    private static TransaccionListDTO MapearLista(Transaccion t) => new() {
        Id = t.Id, Codigo = t.Codigo, EstadoActual = t.EstadoActual,
        MontoOrigen = t.MontoOrigen, MontoDestino = t.MontoDestino,
        TipoCambioAplicado = t.TipoCambioAplicado, FechaInicio = t.FechaInicio,
        MonedaOrigen = t.OfertaCompra?.MonedaOrigen?.Codigo,
        MonedaDestino = t.OfertaCompra?.MonedaDestino?.Codigo,
        NombreComprador = t.Comprador?.Nombres + " " + t.Comprador?.Apellidos,
        NombreVendedor = t.Vendedor?.Nombres + " " + t.Vendedor?.Apellidos,
        CompradorId = t.CompradorId, VendedorId = t.VendedorId
    };

    private static TransaccionDetalleDTO MapearDetalle(Transaccion t) => new() {
        Id = t.Id, Codigo = t.Codigo, EstadoActual = t.EstadoActual,
        MontoOrigen = t.MontoOrigen, MontoDestino = t.MontoDestino,
        TipoCambioAplicado = t.TipoCambioAplicado, FechaInicio = t.FechaInicio,
        MonedaOrigen = t.OfertaCompra?.MonedaOrigen?.Codigo,
        MonedaDestino = t.OfertaCompra?.MonedaDestino?.Codigo,
        NombreComprador = t.Comprador?.Nombres + " " + t.Comprador?.Apellidos,
        NombreVendedor = t.Vendedor?.Nombres + " " + t.Vendedor?.Apellidos,
        CompradorId = t.CompradorId, VendedorId = t.VendedorId,
        Comprobantes = t.Comprobantes.Select(c => new ComprobanteListDTO {
            Id = c.Id, NombreArchivo = c.NombreArchivo, RutaArchivo = c.RutaArchivo,
            FormatoArchivo = c.FormatoArchivo, NumeroOperacion = c.NumeroOperacion,
            FechaTransferencia = c.FechaTransferencia, FechaSubida = c.FechaSubida
        }).ToList()
    };
}
