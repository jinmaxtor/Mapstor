using Example.DTOs;
using Mapstor.Attributes;

namespace Example.Entities;

[MapTo<UserDTO>]
[MapTo<AdminDTO>]
public class User
{
    public int Id { get; set; }
    [MapMember<AdminDTO>(nameof(AdminDTO.NombreCompleto))]
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    [MapIgnore<AdminDTO>]
    public DateOnly FechaNacimiento { get; set; }
}
