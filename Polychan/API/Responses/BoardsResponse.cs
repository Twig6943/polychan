using Newtonsoft.Json;
using Polychan.API.Models;

namespace Polychan.API.Responses;

public struct BoardsResponse
{
    [JsonProperty("boards")]
    public List<Board> Boards;
}