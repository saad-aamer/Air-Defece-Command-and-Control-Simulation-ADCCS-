using Microsoft.AspNetCore.SignalR;
using ADCCS_Web.Models;

namespace ADCCS_Web.Hubs
{
    public class RadarHub : Hub
    {
        // Send target update to all connected clients
        public async Task SendTargetUpdate(object target)
        {
            await Clients.All.SendAsync("ReceiveTargetUpdate", target);
        }

        // Send new target detection to all clients
        public async Task SendNewTarget(object target)
        {
            await Clients.All.SendAsync("ReceiveNewTarget", target);
        }

        // Send target removal notification
        public async Task SendTargetRemoved(int targetId)
        {
            await Clients.All.SendAsync("ReceiveTargetRemoved", targetId);
        }

        // Send alert to all clients
        public async Task SendAlert(object alert)
        {
            await Clients.All.SendAsync("ReceiveAlert", alert);
        }

        // Send classification change
        public async Task SendClassificationChange(int targetId, string newClassification)
        {
            await Clients.All.SendAsync("ReceiveClassificationChange", targetId, newClassification);
        }

        // Notify when a new action is issued
        public async Task SendActionIssued(object action)
        {
            await Clients.All.SendAsync("ReceiveActionIssued", action);
        }

        // Override OnConnectedAsync to log connections
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            // You can add logging here if needed
        }

        // Override OnDisconnectedAsync to log disconnections
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            // You can add logging here if needed
        }
    }
}