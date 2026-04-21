namespace Example.DTOs;

public class AdminDTO
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
}
