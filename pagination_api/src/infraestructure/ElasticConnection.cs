using Elasticsearch.Net;
using System;

// Maneja la conexión de bajo nivel con Elasticsearch usando autenticación básica
public class ElasticConnection : IDisposable
{
    private readonly ElasticLowLevelClient _client;
    private bool _disposed = false;

    public ElasticConnection(string cloudUrl, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(cloudUrl))
            throw new ArgumentException("URL de Elasticsearch no puede ser nula o vacía.", nameof(cloudUrl));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("El usuario no puede ser nulo o vacío.", nameof(username));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede ser nula o vacía.", nameof(password));

        var pool = new SingleNodeConnectionPool(new Uri(cloudUrl));
        var connectionSettings = new ConnectionConfiguration(pool)
            .BasicAuthentication(username, password);

        _client = new ElasticLowLevelClient(connectionSettings);
    }

    // Expone el cliente de conexión si aún no ha sido liberado
    public ElasticLowLevelClient Client
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ElasticConnection));
            return _client;
        }
    }

    // Liberación de recursos (patrón IDisposable)
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Descarta cliente si implementa IDisposable
                (_client as IDisposable)?.Dispose();
            }
            _disposed = true;
        }
    }

    ~ElasticConnection()
    {
        Dispose(false);
    }
}
