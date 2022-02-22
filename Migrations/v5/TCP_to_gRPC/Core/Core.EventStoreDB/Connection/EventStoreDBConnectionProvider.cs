using System;
using System.Threading;
using EventStore.ClientAPI;

namespace Core.EventStoreDB.Connection;

public class EventStoreDBConnectionProvider
{
    private readonly object reconnectLock = new();
    private IEventStoreConnection? instance;
    private string? eventStoreDBUri;

    public IEventStoreConnection Connect(string uri)
    {
        if (instance != null && eventStoreDBUri == uri)
            return instance;

        instance?.Close();

        return Reconnect(uri);
    }

    private IEventStoreConnection Reconnect(string uri)
    {
        try
        {
            Monitor.Enter(reconnectLock);

            var temp = EventStoreConnection.Create(uri);

            Interlocked.Exchange(ref instance, temp);
            Interlocked.Exchange(ref eventStoreDBUri, uri);
        }
        finally
        {
            Monitor.Exit(reconnectLock);
        }

        instance.Closed += (sth, args) =>
        {
            Interlocked.Exchange(ref eventStoreDBUri, null);
            Reconnect(uri);
        };

        // No synchronization context is needed to disable synchronization context.
        // That enables running asynchronous method not causing deadlocks.
        // It's safe if it's run in WebAPI, for UI apps it may cause troubles.
        // But you shouldn't be calling db directly from UI client, right? ;)
        using (NoSynchronizationContextScope.Enter())
        {
            instance.ConnectAsync().Wait();
        }

        return instance;
    }
}

public static class NoSynchronizationContextScope
{
    public static Disposable Enter()
    {
        var context = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        return new Disposable(context);
    }

    public struct Disposable: IDisposable
    {
        private readonly SynchronizationContext? synchronizationContext;

        public Disposable(SynchronizationContext? synchronizationContext)
        {
            this.synchronizationContext = synchronizationContext;
        }

        public void Dispose() =>
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
    }
}
