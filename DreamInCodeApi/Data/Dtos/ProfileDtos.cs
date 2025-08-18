namespace DreamInCodeApi.Data.Dtos;

public record ProfileUpsertRequest(
    string? Idioma,
    string? Voz,
    string? TamanoTexto,
    bool? AltoContraste,
    bool? Consentimiento,
    string? Preferencias,
    string? Notas
);