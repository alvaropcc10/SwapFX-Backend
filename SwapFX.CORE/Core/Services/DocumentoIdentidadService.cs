using SwapFX.CORE.Core.Common;
using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Entities;
using SwapFX.CORE.Core.Interfaces;
namespace SwapFX.CORE.Core.Services;
public class DocumentoIdentidadService : IDocumentoIdentidadService
{
    private readonly IDocumentoIdentidadRepository _documentos;
    public DocumentoIdentidadService(IDocumentoIdentidadRepository documentos) { _documentos = documentos; }

    public async Task<RespuestaApi<bool>> SubirAsync(int usuarioId, SubirDocumentoDTO dto)
    {
        await _documentos.AddAsync(new DocumentoIdentidad {
            UsuarioId = usuarioId,
            TipoDoc = dto.TipoDoc,
            NumeroDoc = dto.NumeroDoc,
            RutaArchivo = dto.RutaArchivo,
            Estado = dto.Estado,
            FechaSubida = DateTime.UtcNow
        });
        return RespuestaApi<bool>.Ok(true, "Documento enviado para revisión.");
    }
}
