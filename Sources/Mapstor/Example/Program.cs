using Example.DTOs;
using Example.Entities;

Console.WriteLine("Hello, World!");


var usuario = new User
{
    Id = 1,
    Nombre = "John",
    Apellidos = "Doe",
    Correo = "prueba@gmail.com",
    FechaNacimiento = new DateOnly(1995, 9, 6)
};

//var usuarioDTO = usuario.MapTo<UserDTO>();
//var adminDTO = usuario.MapTo<AdminDTO>();

//Console.WriteLine(usuarioDTO);
//Console.WriteLine(adminDTO);