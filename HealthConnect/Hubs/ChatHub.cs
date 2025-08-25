using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HealthConnect.Hubs
{
    public class ChatHub : Hub
    {
        // Send the message to all clients (broadcast)
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", senderId, receiverId, message);
        }

        // WebRTC Signaling Methods (optional, based on your requirements)
        public async Task SendOffer(string targetUser, string offer)
        {
            await Clients.Client(targetUser).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }

        public async Task SendAnswer(string targetUser, string answer)
        {
            await Clients.Client(targetUser).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIceCandidate(string targetUser, string candidate)
        {
            await Clients.Client(targetUser).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }
    }
}
