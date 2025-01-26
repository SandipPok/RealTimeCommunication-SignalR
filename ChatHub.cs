using Microsoft.AspNetCore.SignalR;
using RealTimeCommunication_SignalR.Model;

namespace RealTimeCommunication_SignalR
{
    public class ChatHub : Hub
    {
        private readonly SharedDb _db;

        public ChatHub(SharedDb db) => _db = db;

        public async Task JoinChat(UserConnection conn)
        {
            await Clients.All.SendAsync("ReceiveMessage", "admin", $"{conn.UserName} has joined the chat.");
        }

        public async Task JoinSpecificChatRoom(UserConnection conn)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.ChatRoom);

            _db.Connection[Context.ConnectionId] = conn;

            await Clients.Groups(conn.ChatRoom).SendAsync("JoinSpecificChatRoom", "admin", $"{conn.UserName} has joined the chat room {conn.ChatRoom}.");
        }

        public async Task SendMessage(string msg)
        {
            if (_db.Connection.TryGetValue(Context.ConnectionId, out UserConnection? conn))
            {
                await Clients.Group(conn.ChatRoom).SendAsync("ReceiveSpecificMessage", conn.UserName, msg);
            }
        }
    }
}
