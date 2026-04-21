# Mapstor  
High‑performance object mapping for .NET powered by Source Generators.

## 🚀 Overview
**Mapstor** is a compile‑time object mapping library for .NET.  
It generates strongly‑typed mapping code using **Source Generators**, eliminating reflection and delivering **near‑zero overhead** at runtime.

Designed for APIs, microservices, and high‑throughput applications where performance and type safety matter.

## ✨ Features
- ⚡ Ultra‑fast: mapping code is generated at compile time  
- 🧩 Type‑safe: errors are caught during compilation  
- 🛠️ Supports multiple target types via attributes  
- 📦 Zero dependencies  
- 🔍 AOT / NativeAOT friendly  
- 🔄 Incremental generator: only regenerates what changed  
- 🧬 Works with classes, records, and structs  

## 📦 Installation
```bash
dotnet add package Mapstor
```

## Usage Example

### 1. Entity with mapping attributes
```csharp
using Example.DTOs;
using Mapstor.Attributes;

namespace Example.Entities;

[MapTo<UserDTO>]
[MapTo<AdminDTO>]
public class User
{
    public int Id { get; set; }

    // Maps to AdminDTO.NombreCompleto
    [MapMember<AdminDTO>(nameof(AdminDTO.NombreCompleto))]
    public string Nombre { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    // Ignored only when mapping to AdminDTO
    [MapIgnore<AdminDTO>]
    public DateOnly FechaNacimiento { get; set; }
}
```

### 2. Define your DTOs
```csharp
namespace Example.DTOs;

public class UserDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
}

public class AdminDTO
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
}
```

### 3. Use the generated mapping methods
```csharp
var usuario = new User
{
    Id = 1,
    Nombre = "John",
    Apellidos = "Doe",
    Correo = "prueba@gmail.com",
    FechaNacimiento = new DateOnly(1995, 9, 6)
};

var usuarioDTO = usuario.MapTo<UserDTO>();
var adminDTO = usuario.MapTo<AdminDTO>();
```
