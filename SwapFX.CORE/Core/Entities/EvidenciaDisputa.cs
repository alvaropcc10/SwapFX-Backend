namespace SwapFX.CORE.Core.Entities;
public partial class EvidenciaDisputa
{
    public int Id { get; set; }
    public int DisputaId { get; set; }
    public int UsuarioId { get; set; }
    public string NombreArchivo { get; set; } = null!;
    public string RutaArchivo { get; set; } = null!;
    public string FormatoArchivo { get; set; } = null!;
    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
    public virtual Disputa? Disputa { get; set; }
}
