using KarlCube.Games.Shared;

namespace KarlCube.Games.Achtung;

public class AchtungGame
{
    private const int Matrix = 64;
    private const int Rows = Matrix*5;
    private const int Cols = Matrix;
    
    private static readonly Random Random = new();
    private static readonly object SyncLock = new();

    private static int RandomNumber(int min, int max)
    {
        lock(SyncLock) {
            return Random.Next(min, max);
        }
    }

    public AchtungGameContext CreateGameContext()
    {
        var map = new GameObject[Rows,Cols];

        var playerOne = new Player
        {
            Id = 1,
            Direction = Direction.Up,
            Position = ((Matrix/2) -1, Matrix/2),
            Color = GameObject.RedDot
        };
        var playerTwo = new Player
        {
            Id = 2,
            Direction = Direction.Down,
            Position = ((Matrix/2) +1, Matrix/2),
            Color = GameObject.BlueDot
        };
        map[playerOne.Position.Item1, playerOne.Position.Item2] = playerOne.Color;
        map[playerTwo.Position.Item1, playerTwo.Position.Item2] = playerTwo.Color;
        return new AchtungGameContext
        {
            Map = map,
            Players = new []
            {
                playerOne,
                playerTwo
            }
        };
    }
    
    public AchtungGameContext Loop(AchtungGameContext context)
    {
        foreach (var player in context.Players)
        {
            var (x, y) = player.Position;
            try
            {
                var (nx, ny) = player.Direction switch
                {
                    Direction.Up => (x - 1, y),
                    Direction.Down => (x + 1, y),
                    Direction.Left => (x, y - 1),
                    Direction.Right => (x, y + 1),
                    Direction.None => (x, y),
                    _ => throw new ArgumentOutOfRangeException()
                };
                var (newX, newY, newDirection) = GetDirection.FindNextPosition((x, y), (nx, ny), player.Direction);
                var obj = context.Map[newX, newY];

                if (obj == GameObject.Ground)
                {
                    context.Map = ModifyHitGround(context.Map, player);
                    player.Position = (newX, newY);
                    player.Direction = newDirection;
                    if (player.MakeGap > 0) {
                        player.MakeGap--;
                    }
                    if (player.MakeGap != 0) continue;
                    if (RandomNumber(0, 300) == 100)
                    {
                        player.MakeGap = 3;
                    }
                }
                else
                {
                    player.Dead = true;
                    player.Position = (newX, newY);
                }
            }
            catch (Exception e)
            {
                player.Dead = true;
            }
        }
        
        return context;
    }
    
    private static GameObject[,] ModifyHitGround(GameObject[,] map, Player player)
    {
        if (player.MakeGap == 0)
        {
            map[player.Position.Item1, player.Position.Item2] = player.Color;
        }
        return map;
    }
}