public record GameContext
{
    public (GameObject, Direction)[,] Map { get; init; } = null!;
    public Direction Direction { get; init; }
    public (int, int) Position { get; init; }
    public int Score { get; init; }
    public int StepsLeft { get; init; }
    public bool Dead { get; init; }
    public NetCoreAudio.Player AudioPlayer { get; set; }
}