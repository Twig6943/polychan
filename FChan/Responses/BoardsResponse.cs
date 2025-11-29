using Newtonsoft.Json;
using FChan.Models;

namespace FChan.Responses;

public struct BoardsResponse
{
    [JsonProperty("boards")]
    public List<Board> Boards;
}