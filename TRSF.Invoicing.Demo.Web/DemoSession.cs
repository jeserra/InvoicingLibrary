using System.Collections.Concurrent;
using System.IO;
using TRSF.Invoicing.Catalogs.Sqlite;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Demo.Web
{
    /// <summary>
    /// Estado de una corrida del wizard, en memoria unicamente. No hay base de datos ni
    /// autenticacion - es un demo de un solo usuario a la vez por sesion, no un sistema
    /// multiusuario. Nada aqui (certificado, llave, PDF de constancia) se escribe a disco
    /// salvo el .sqlite curado, que se genera en un archivo temporal y se borra al limpiar
    /// la sesion.
    /// </summary>
    public class DemoSession
    {
        public required string Id { get; init; }
        public required string CuratedDbPath { get; init; }
        public required SqliteCatalogValidator CatalogValidator { get; init; }

        public BindingModels.Emisor? Emisor { get; set; }
        public ICertificatesRepository? CertificatesRepository { get; set; }
        public string? NoCertificado { get; set; }

        public BindingModels.Receptor? Receptor { get; set; }

        public string? SelloXml { get; set; }
    }

    public class DemoSessionStore
    {
        private readonly ConcurrentDictionary<string, DemoSession> sessions = new();

        public DemoSession Create(string curatedDbPath, SqliteCatalogValidator validator)
        {
            var session = new DemoSession
            {
                Id = Guid.NewGuid().ToString("N"),
                CuratedDbPath = curatedDbPath,
                CatalogValidator = validator,
            };
            sessions[session.Id] = session;
            return session;
        }

        public DemoSession? Get(string id) => sessions.TryGetValue(id, out var session) ? session : null;

        public void Remove(string id)
        {
            if (sessions.TryRemove(id, out var session))
            {
                session.CatalogValidator.Dispose();
                if (File.Exists(session.CuratedDbPath))
                    File.Delete(session.CuratedDbPath);
            }
        }
    }
}
