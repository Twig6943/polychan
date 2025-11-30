using Newtonsoft.Json;
using Backends.FChan.Models;

namespace Backends.FChan.Responses;

public struct BoardsResponse
{
    [JsonProperty("boards")]
    public List<Board> Boards;
}