using FeatureProbe.Server.Sdk.DataRepositories;
using FeatureProbe.Server.Sdk.Internal;
using Microsoft.Extensions.Logging;
using SocketIOClient;
using SocketIOClient.Transport;

namespace FeatureProbe.Server.Sdk.Synchronizer;

public class StreamingSynchronizer : ISynchronizer
{
    private readonly PollingSynchronizer _pollingSynchronizer;

    private readonly SocketIO _socket;

    internal StreamingSynchronizer(FPConfig config, IDataRepository dataRepo)
    {
        _pollingSynchronizer = new PollingSynchronizer(config, dataRepo);
        _socket = ConnectSocket(config);
    }

    internal StreamingSynchronizer(FPConfig config, PollingSynchronizer synchronizer)
    {
        _pollingSynchronizer = synchronizer;
        _socket = ConnectSocket(config);
    }

    public Task SynchronizeAsync() => _pollingSynchronizer.SynchronizeAsync();

    public async ValueTask DisposeAsync()
    {
        await _pollingSynchronizer.DisposeAsync();
        await _socket.DisconnectAsync();
    }

    private SocketIO ConnectSocket(FPConfig config)
    {
        var options = new SocketIOOptions
        {
            Transport = TransportProtocol.WebSocket, Path = new Uri(config.RealtimeUrl).LocalPath
        };
        var socket = new SocketIO(config.RealtimeUrl, options);

        socket.OnConnected += async (sender, args) =>
        {
            Loggers.Synchronizer?.Log(LogLevel.Information, "Connect socket success");
            var credentials = new Dictionary<string, string> { { "key", config.ServerSdkKey } };
            await socket.EmitAsync("register", credentials);
        };

        socket.OnDisconnected += (sender, args) =>
            Loggers.Synchronizer?.Log(LogLevel.Information, "Socket disconnected");

        socket.OnError += (sender, args) =>
            Loggers.Synchronizer?.Log(LogLevel.Error, "Socket error: {args}", args);

        socket.On("update", async (resp) =>
        {
            Loggers.Synchronizer?.Log(LogLevel.Information, "Socket received update event");
            await _pollingSynchronizer.PollAsync();
        });

        socket.ConnectAsync().Wait();

        return socket;
    }
}

public class StreamingSynchronizerFactory : ISynchronizerFactory
{
    public ISynchronizer Create(FPConfig config, IDataRepository dataRepo)
    {
        return new StreamingSynchronizer(config, dataRepo);
    }
}
