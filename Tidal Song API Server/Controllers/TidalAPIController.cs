using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

public class SongData
{
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }

    [JsonPropertyName("trackId")]
    public string TrackId { get; set; }

    [JsonPropertyName("trackUrl")]
    public string TrackUrl { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("artists")]
    public string[] Artists { get; set; }

    [JsonPropertyName("artistsString")]
    public string ArtistsString { get; set; }
}

public class Status
{
    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("songData")]
    public SongData SongData { get; set; }
}

namespace Tidal_Song_API_Server.Controllers
{
    [ApiController]
    public class TidalAPIController : ControllerBase
    {
        private static readonly string authorizationToken = ""; // Generate a random token, make sure to set it for your client too
        private static Status? status;

        [HttpPost("status")]
        public async Task<object> SetStatus([FromBody] JsonElement status) {
            if (!Request.Headers.ContainsKey("Authorization") || Request.Headers["Authorization"].FirstOrDefault(e => e == authorizationToken) == null)
            {
                return StatusCode((int)HttpStatusCode.Unauthorized);
            }

            TidalAPIController.status = status.Deserialize<Status>();
            await WebSocketManager.SendMessageToAllAsync(JsonSerializer.Serialize(status));

            return Ok();
        }

        [HttpGet("status")]
        public Status? GetStatus()
        {
            return status;
        }
    }
}
