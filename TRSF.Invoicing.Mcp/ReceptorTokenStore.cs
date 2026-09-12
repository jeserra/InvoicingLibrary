using System.Collections.Concurrent;
using ModelContextProtocol;

namespace TRSF.Invoicing.Mcp;

/// <summary>
/// Guarda temporalmente los datos de Receptor leidos por read_constancia_fiscal, referenciables
/// despues por un token opaco desde create_invoice. Evita que RFC/nombre/domicilio del receptor
/// tengan que pasar por el contexto del LLM en cada llamada, igual que cer.rfc evita repetir la
/// contrasena del CSD. Almacenamiento en memoria del proceso: no sobrevive un reinicio del server,
/// suficiente porque el token solo necesita vivir lo que dura una conversacion.
/// </summary>
public static class ReceptorTokenStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);
    private static readonly ConcurrentDictionary<string, (ReceptorInput Receptor, DateTimeOffset Expires)> _store = new();

    public static string Save(ReceptorInput receptor)
    {
        var token = Guid.NewGuid().ToString("N");
        _store[token] = (receptor, DateTimeOffset.UtcNow.Add(Ttl));
        return token;
    }

    public static ReceptorInput Resolve(string token)
    {
        if (!_store.TryGetValue(token, out var entry))
            throw new McpException($"Token de receptor desconocido o ya expirado: '{token}'. Vuelva a leer la constancia fiscal.");

        if (entry.Expires < DateTimeOffset.UtcNow)
        {
            _store.TryRemove(token, out _);
            throw new McpException($"El token de receptor expiro: '{token}'. Vuelva a leer la constancia fiscal.");
        }

        return entry.Receptor;
    }
}
