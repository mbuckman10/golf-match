using Microsoft.AspNetCore.SignalR;

namespace GolfMatchPro.Api.Hubs;

public class MatchHub : Hub
{
    public async Task JoinMatch(int matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");
    }

    public async Task LeaveMatch(int matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match-{matchId}");
    }
}
