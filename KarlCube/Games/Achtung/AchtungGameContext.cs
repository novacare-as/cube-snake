using KarlCube.Games.Shared;

namespace KarlCube.Games.Achtung;

public record AchtungGameContext
{
    public GameObject[,] Map { get; set; } = null!;
    public IEnumerable<Player> Players { get; init; }
}

public record Player
{
    public Direction Direction { get; set; }
    public (int, int) Position { get; set; }
    public (int, int, int) Color { get; set; }
    public int MakeGap { get; set; }
    public bool Dead { get; set; }
}