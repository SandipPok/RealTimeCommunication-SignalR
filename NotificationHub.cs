using Microsoft.AspNetCore.SignalR;
using RealTimeCommunication_SignalR.Model;

namespace RealTimeCommunication_SignalR
{
    public class NotificationHub : Hub
    {
        private readonly SharedDb _db;

        public NotificationHub(SharedDb db)
        {
            _db = db;
        }
        public async Task SendNotification(string message)
        {
            if (_db.Connection.TryGetValue(Context.ConnectionId, out UserConnection? conn))
            {
                await Clients.Group(conn.ChatRoom).SendAsync("ReceiveNotification", conn.UserName, message);
            }
        }
        public async Task JoinGroup(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);
            _db.Connection[Context.ConnectionId] = conn;
            await Clients.Group(conn.ChatRoom).SendAsync("ReceiveNotification", "admin", $"{conn.UserName} joined notifications in {conn.ChatRoom}.");
        }
    }
}