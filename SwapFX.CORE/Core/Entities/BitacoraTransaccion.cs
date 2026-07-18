namespace SwapFX.CORE.Core.Entities;
public partial class BitacoraTransaccion
{
    public int Id { get; set; }
    public int TransaccionId { get; set; }
    public string EstadoAnterior { get; set; } = null!;
    public string EstadoNuevo { get; set; } = null!;
    public int UsuarioResponsableId { get; set; }
    public string? Comentario { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;
    public virtual Transaccion? Transaccion { get; set; }
}
