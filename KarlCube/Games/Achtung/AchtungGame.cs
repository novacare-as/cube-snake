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
            Direction = Direction.Up,
            Position = (Matrix * 2 + Matrix / 2, Matrix / 2),
            Color = RandomBrightColor()
        };
        var playerTwo = new Player{
            Direction = Direction.Down,
            Position = (Matrix * 4 + Matrix / 2, Matrix / 2),
            Color = RandomBrightColor()
        };
        map[playerOne.Position.Item1, playerOne.Position.Item2] = GameObject.Dot;
        map[playerTwo.Position.Item1, playerTwo.Position.Item2] = GameObject.Dot;
        return new AchtungGameContext
        {
            Map = map,
            Players = new []{ playerOne, playerTwo }
        };
    }
    
    public AchtungGameContext Loop(AchtungGameContext context)
    {
        foreach (var player in context.Players)
        {
            var (x, y) = player.Position;
            var (nx, ny) = player.Direction switch
            {
                Direction.Up => (x - 1, y),
                Direction.Down => (x + 1, y),
                Direction.Left => (x, y - 1),
                Direction.Right => (x, y + 1),
                Direction.None => (x, y),
                _ => throw new ArgumentOutOfRangeException()
            };

            var (newX, newY, _) = GetDirection.FindNextPosition((x, y), (nx, ny), player.Direction);
            var obj = context.Map[newX, newY];

            if (obj == GameObject.Ground)
            {
                context.Map = ModifyHitGround(context.Map, (newX, newY), player.MakeGap);
                player.Position = (newX, newY);
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
        
        return context;
    }
    
    private static GameObject[,] ModifyHitGround(GameObject[,] map, (int x, int y) position, int makeGap)
    {
        if (makeGap == 0)
        {
            map[position.x, position.y] = GameObject.Dot;
        }
        return map;
    }

    private static (int, int, int) RandomBrightColor()
    {
        var r = RandomNumber(127, 255);
        var g = RandomNumber(127, 255);
        var b = RandomNumber(127, 255);
        return (r, g, b);
    }
}