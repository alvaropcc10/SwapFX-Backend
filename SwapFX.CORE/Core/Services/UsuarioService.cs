using SwapFX.CORE.Core.DTOs;
using SwapFX.CORE.Core.Entities;
using SwapFX.CORE.Core.Interfaces;
namespace SwapFX.CORE.Core.Services;
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJWTService _jwtService;
    public UsuarioService(IUsuarioRepository usuarioRepository, IJWTService jwtService)
    {
        _usuarioRepository = usuarioRepository;
        _jwtService = jwtService;
    }
    public async Task<UsuarioListDTO?> SignIn(UsuarioSignInDTO dto)
    {
        var usuario = await _usuarioRepository.SignIn(dto.Email, dto.Password);
        if (usuario == null) return null;
        var token = _jwtService.GenerateJWToken(usuario);
        return new UsuarioListDTO {
            Id = usuario.Id,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Email = usuario.Email,
            Tipo = usuario.Tipo,
            IdentidadValidada = usuario.IdentidadValidada,
            Token = token
        };
    }
    public async Task<int> SignUp(UsuarioSignUpDTO dto)
    {
        var usuario = new Usuario {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Dni = dto.Dni,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Password = dto.Password,
            IsActive = true,
            Tipo = "U",
            FechaRegistro = DateOnly.FromDateTime(DateTime.UtcNow),
            IdentidadValidada = false
        };
        return await _usuarioRepository.SignUp(usuario);
    }
}
