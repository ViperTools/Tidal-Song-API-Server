using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

public static class WebSocketManager
{
    private static readonly ConcurrentDictionary<Guid, WebSocket> connections = new();

    public static Guid AddWebSocket(WebSocket socket)
    {
        Guid guid = Guid.NewGuid();
        connections.TryAdd(guid, socket);

        return guid;
    }

    public static async Task RemoveWebSocketAsync(Guid id)
    {
        if (!connections.TryRemove(id, out WebSocket? socket))
        {
            return;
        }

        if (socket?.State == WebSocketState.Open)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

        socket?.Dispose();
    }

    public static async Task SendMessageAsync(Guid id, string message)
    {
        if (!connections.TryGetValue(id, out WebSocket? socket)) {
            return;
        }

        await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public static async Task SendMessageToAllAsync(string message)
    {
        foreach (Guid guid in connections.Keys)
        {
            if (connections[guid].State == WebSocketState.Open)
            {
                await SendMessageAsync(guid, message);
            }
            else
            {
                await RemoveWebSocketAsync(guid);
            }
        }
    }

    public static async Task HandleMessages(Guid id)
    {
        if (!connections.TryGetValue(id, out WebSocket? socket))
        {
            return;
        }

        while (true)
        {
            ArraySegment<byte> buffer = new(new byte[0], 0, 0);

            if ((await socket.ReceiveAsync(buffer, CancellationToken.None)).CloseStatus != null)
            {
                break;
            }
        }
    }
}