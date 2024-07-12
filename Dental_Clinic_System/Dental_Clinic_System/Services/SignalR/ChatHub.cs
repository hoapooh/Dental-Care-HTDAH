//using Dental_Clinic_System.Helper;
//using Microsoft.AspNetCore.SignalR;
//using System.Collections.Concurrent;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Dental_Clinic_System.Models.Data
//{
//    public class ChatHub : Hub
//    {
//        private static ConcurrentDictionary<string, string> userConnections = new ConcurrentDictionary<string, string>();
//        private readonly DentalClinicDbContext _context;

//        public ChatHub(DentalClinicDbContext context)
//        {
//            _context = context;
//        }

//        public override Task OnConnectedAsync()
//        {
//            var userID = Context.GetHttpContext().Request.Query["userID"];
//            userConnections[Context.ConnectionId] = userID;
//            System.Diagnostics.Debug.WriteLine($"User connected: {userID} with connection ID: {Context.ConnectionId}");
//            return base.OnConnectedAsync();
//        }

//        public override Task OnDisconnectedAsync(Exception exception)
//        {
//            userConnections.TryRemove(Context.ConnectionId, out _);
//            System.Diagnostics.Debug.WriteLine($"User disconnected with connection ID: {Context.ConnectionId}");
//            return base.OnDisconnectedAsync(exception);
//        }

//        public async Task SendMessageToUser(string receiverID, string message)
//        {
//            var senderID = userConnections[Context.ConnectionId];
//            System.Diagnostics.Debug.WriteLine($"Sending message from {senderID} to {receiverID}: {message}");

//            try
//            {
//                var sender = _context.Accounts.FirstOrDefault(u => u.ID.ToString() == senderID);
//                var receiver = _context.Accounts.FirstOrDefault(u => u.ID.ToString() == receiverID);

//                if (sender != null && receiver != null)
//                {
//                    var chatMessage = new ChatHubMessage
//                    {
//                        Content = message,
//                        Timestamp = Util.GetUtcPlus7Time(),
//                        SenderId = sender.ID,
//                        ReceiverId = receiver.ID
//                    };

//                    _context.ChatHubMessages.Add(chatMessage);
//                    await _context.SaveChangesAsync();

//                    string roomName = GetRoomName(sender.ID, receiver.ID);

//                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
//                    var receiverConnectionId = userConnections.FirstOrDefault(x => x.Value == receiverID).Key;
//                    if (receiverConnectionId != null)
//                    {
//                        await Groups.AddToGroupAsync(receiverConnectionId, roomName);
//                    }

//                    await Clients.Group(roomName).SendAsync("ReceiveMessage", senderID, message);
//                    System.Diagnostics.Debug.WriteLine($"Message sent to room {roomName}: {message}");
//                }
//                else
//                {
//                    System.Diagnostics.Debug.WriteLine($"Either sender or receiver is null. Sender: {senderID}, Receiver: {receiverID}");
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Error in SendMessageToUser: {ex.Message}");
//                throw;
//            }
//        }

//        private string GetRoomName(int userId1, int userId2)
//        {
//            var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id).ToArray();
//            return $"{sortedIds[0]}-{sortedIds[1]}";
//        }
//    }
//}


// aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa



using Dental_Clinic_System.Helper;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Dental_Clinic_System.Models.Data
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> userConnections = new ConcurrentDictionary<string, string>();
        private readonly DentalClinicDbContext _context;

        public ChatHub(DentalClinicDbContext context)
        {
            _context = context;
        }

        public override Task OnConnectedAsync()
        {
            var userID = Context.GetHttpContext().Request.Query["userID"];
            userConnections[Context.ConnectionId] = userID;
            System.Diagnostics.Debug.WriteLine($"User connected: {userID} with connection ID: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            userConnections.TryRemove(Context.ConnectionId, out _);
            System.Diagnostics.Debug.WriteLine($"User disconnected with connection ID: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToUser(string receiverID, string message)
        {
            var senderID = userConnections[Context.ConnectionId];
            System.Diagnostics.Debug.WriteLine($"Sending message from {senderID} to {receiverID}: {message}");

            try
            {
                var sender = _context.Accounts.FirstOrDefault(u => u.ID.ToString() == senderID);
                var receiver = _context.Accounts.FirstOrDefault(u => u.ID.ToString() == receiverID);

                if (sender != null && receiver != null)
                {
                    var chatMessage = new ChatHubMessage
                    {
                        Content = message,
                        Timestamp = Util.GetUtcPlus7Time(),
                        SenderId = sender.ID,
                        ReceiverId = receiver.ID
                    };

                    _context.ChatHubMessages.Add(chatMessage);
                    await _context.SaveChangesAsync();

                    string roomName = GetRoomName(sender.ID, receiver.ID);

                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                    var receiverConnectionId = userConnections.FirstOrDefault(x => x.Value == receiverID).Key;
                    if (receiverConnectionId != null)
                    {
                        await Groups.AddToGroupAsync(receiverConnectionId, roomName);
                    }

                    await Clients.Group(roomName).SendAsync("ReceiveMessage", senderID, message);
                    System.Diagnostics.Debug.WriteLine($"Message sent to room {roomName}: {message}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Either sender or receiver is null. Sender: {senderID}, Receiver: {receiverID}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendMessageToUser: {ex.Message}");
                throw;
            }
        }

        private string GetRoomName(int userId1, int userId2)
        {
            var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id).ToArray();
            return $"{sortedIds[0]}-{sortedIds[1]}";
        }
    }
}
