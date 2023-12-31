using Microsoft.AspNetCore.SignalR;

namespace FlutterSignalIR.Hubs
{
    public  class TestHub2:Hub<ITest2Client>
    {

        public async Task SendMessage(string message)
        {
            // Broadcast the received message to all connected clients
            await Clients.All.ReeciveMessage($"{Context.ConnectionId}: {message}");
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.ReeciveMessage($"{Context.ConnectionId}");
        }

        

    }
}
