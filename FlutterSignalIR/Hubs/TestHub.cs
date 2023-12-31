using Microsoft.AspNetCore.SignalR;

namespace FlutterSignalIR.Hubs
{
    public  class TestHub:Hub
    {

        public async Task Oku()
        {
            await Clients.Others.SendAsync("Mesaj", "Ben mansur");
            Console.WriteLine("FLUTTER");
        }

        //public async Task UyariVer(string ids, string currentCookeId, string currentConnectId)
        //{
        //    await Clients.Groups(currentCookeId).SendAsync("UyariMesaji", "Mesaj geldi..");
        //}

        public async Task UyariVer()
        {
            await Clients.All.SendAsync("UyariMesaji", "Mesaj geldi..");
        }
        public async Task SendMessage(string user, string message)
        {
            // Broadcast the received message to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            //await Clients.All.SendAsync("UyariMesaji", "Mesaj geldi..");
        }

       

    }
}
