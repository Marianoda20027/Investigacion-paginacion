using Elasticsearch.Net;
using System;

public class ElasticConnection : IDisposable
{
    private readonly ElasticLowLevelClient _client;
    private bool _disposed = false;

    public ElasticConnection()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var connectionSettings = new ConnectionConfiguration(pool);

        _client = new ElasticLowLevelClient(connectionSettings);
    }

    public ElasticLowLevelClient Client
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ElasticConnection));
            return _client;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            (_client as IDisposable)?.Dispose();
            _disposed = true;
        }
    }

    ~ElasticConnection()
    {
        Dispose(false);
    }
}
