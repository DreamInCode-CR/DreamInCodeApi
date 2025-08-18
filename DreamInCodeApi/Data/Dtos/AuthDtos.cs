namespace DreamInCodeApi.Data.Dtos;

public record RegisterRequest(
    string Correo,
    string Password,
    string? Nombre,
    string? PrimerApellido,
    string? SegundoApellido,
    DateOnly? FechaNacimiento,
    string? Telefono,
    string? Direccion
);

public record LoginRequest(string Correo, string Password);

public record AuthResponse(
    int Id,
    string Correo,
    string? Nombre,
    string? PrimerApellido,
    string? SegundoApellido,
    string Token
);