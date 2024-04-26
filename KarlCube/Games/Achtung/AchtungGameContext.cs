using KarlCube.Games.Shared;

namespace KarlCube.Games.Achtung;

public record AchtungGameContext
{
    public GameObject[,] Map { get; set; } = null!;
    public IEnumerable<Player> Players { get; init; }
}

public record Player
{
    public int Id { get; init; }
    public Direction Direction { get; set; }
    public (int, int) Position { get; set; }
    public GameObject Color { get; init; }
    public int MakeGap { get; set; }
    public bool Dead { get; set; }
}