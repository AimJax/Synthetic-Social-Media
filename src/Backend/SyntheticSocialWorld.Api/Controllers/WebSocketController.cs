using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Primitives;
using SyntheticSocialWorld.Api.Services;
using System.Text;
using System.Text.Json;

namespace SyntheticSocialWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketService _webSocketService;
    private readonly ILogger<WebSocketController> _logger;

    public WebSocketController(
        WebSocketService webSocketService,
        ILogger<WebSocketController> logger)
    {
        _webSocketService = webSocketService;
        _logger = logger;
    }

    [HttpGet("/ws")]
    public async Task WebSocket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        // Get player ID from query string
        var playerId = HttpContext.Request.Query["playerId"].ToString();
        if (string.IsNullOrEmpty(playerId))
        {
            playerId = "anonymous";
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var connectionId = await _webSocketService.AddConnectionAsync(webSocket, playerId);

        _logger.LogInformation("WebSocket connection established: {ConnectionId}", connectionId);

        try
        {
            var buffer = new byte[1024 * 4];
            
            while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    
                    // Handle incoming message
                    var response = await _webSocketService.HandleMessageAsync(connectionId, message);
                    
                    if (response != null)
                    {
                        var responseJson = JsonSerializer.Serialize(response);
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                        
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(responseBytes),
                            System.Net.WebSockets.WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebSocket error for connection {ConnectionId}", connectionId);
        }
        finally
        {
            await _webSocketService.RemoveConnectionAsync(connectionId);
        }
    }

    [HttpGet("connections")]
    public IActionResult GetConnectionCount()
    {
        return Ok(new { count = _webSocketService.ConnectionCount });
    }
}
