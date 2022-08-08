using NetCoreAudio;

public class Game
{
    private const int Matrix = 32;
    private const int Rows = Matrix*5;
    private const int Cols = Matrix;

    private string _audioPath = $"{Directory.GetCurrentDirectory()}\\Audio\\";

    public GameContext CreateGameContext()
    {
        var map = new (GameObject, Direction)[Rows,Cols];
        for (var row = 0; row < Rows; row++)
        for (var column = 0; column < Cols; column++)
            map[row, column] = (GameObject.Ground, Direction.None);

        map[Matrix/2, Matrix/2] = (GameObject.Snake, Direction.Up);
        var (foodX, foodY) = CreateFood(map);
        map[foodX, foodY] = (GameObject.Food, Direction.None);
        return new GameContext
        {
            Map = map,
            Direction = Direction.Up,
            Position = (Matrix/2, Matrix/2),
            Score = 0,
            StepsLeft = 400,
            Dead = false,
            AudioPlayer = new Player()
        };
    }

    public GameContext Loop(GameContext context)
    {
        if (context.StepsLeft == 0)
            return context with { Dead = true };
        
        var (x, y) = context.Position;
        var (nx, ny) = context.Direction switch
        {
            Direction.Up => (x - 1, y),
            Direction.Down => (x + 1, y),
            Direction.Left => (x, y - 1),
            Direction.Right => (x, y + 1),
            Direction.None => (x, y),
            _ => throw new ArgumentOutOfRangeException()
        };

        var (newX, newY, direction) = FindNextPosition((x, y), (nx, ny), context.Direction);
        
        Console.Write($"Direction: {direction}, x: {newX}, y: {newY}\n");
        try
        {
            var (obj, _) = context.Map[newX, newY];

            return obj switch
            {
                GameObject.Ground => context with
                {
                    Map = ModifyHitGround(context, (newX, newY), direction),
                    Position = (newX, newY),
                    StepsLeft = context.StepsLeft - 1,
                    Direction = direction
                },
                GameObject.Snake => context with
                {
                    Dead = true,
                    Position = (newX, newY),
                    Direction = direction
                },
                GameObject.Food => context with
                {
                    Map = ModifyHitFood(context, (newX, newY), direction),
                    Position = (newX, newY),
                    StepsLeft = context.StepsLeft + 100,
                    Score = context.Score + 10,
                    Direction = direction
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (IndexOutOfRangeException e)
        {
            return context with { Dead = true };
        }
    }

    private static (int, int, Direction) FindNextPosition((int x, int y) oldPosition, (int newX, int newY) newPosition, Direction direction)
    {
        var (x, y) = oldPosition;
        var (newX, newY) = newPosition;

        if (newX == Matrix*5 && x == Matrix*5-1)
        {
            return (Matrix, newY, direction);
        }
        if (newX == Matrix-1 && x == Matrix)
        {
            return (Matrix*5-1, newY, direction);
        }

        if (newY is -1 // Might go to top or to side 96-128 if already on top
            || (y == Matrix-1 && newY == Matrix) // From top and goes over the edge to side 32-64
            || newX is -1 or Matrix) // From top to 128-160 (if x is 32) or 64-96 (if x is -1)
        {
            var side = (int) Math.Floor((decimal) (x / 32));
            if (side is 0)
            {
                switch (newY)
                {
                    case -1:
                        return (Matrix*3 + Math.Abs(Matrix-1 - newX), 0, Direction.Right);
                    case Matrix:
                        return (Matrix*1 + newX, 0, Direction.Right);
                }
                switch (newX)
                {
                    case -1:
                        return (Matrix*4 + newY, 0, Direction.Right);
                    case Matrix:
                        return (Matrix*2 + Math.Abs(Matrix-1 - newY), 0, Direction.Right);
                }
            }

            if (side is 1)
            {
                if (newY == -1)
                {
                    return (newX - Matrix, Matrix-1, Direction.Left);
                }
            }
            
            if (side is 3)
            {
                if (newY == -1)
                {
                    return (Math.Abs(Matrix-1 - (newX - Matrix*3)), 0, Direction.Right);
                }
            }
            
            if (side is 2)
            {
                if (newY == -1)
                {
                    return (Matrix-1, Math.Abs(31 - (newX - Matrix*2)), Direction.Up);
                }
            }
            
            if (side is 4)
            {
                if (newY == -1)
                {
                    return (0, newX - Matrix*4, Direction.Down);
                }
            }
            
        }

        return (newX, newY, direction);
    }

    private (GameObject, Direction)[,] ModifyHitFood(GameContext context, (int x, int y) position,
        Direction direction)
    {
        context.Map[position.x, position.y] = (GameObject.Snake, direction);
        var (foodX, foodY) = CreateFood(context.Map);
        context.Map[foodX, foodY] = (GameObject.Food, Direction.None);
        context.AudioPlayer.Play($"{_audioPath}PointGiven.wav");
        return context.Map;
    }

    private static (GameObject, Direction)[,] ModifyHitGround(GameContext context, (int x, int y) position,
        Direction direction)
    {
        context.Map[position.x, position.y] = (GameObject.Snake, direction);
        var (tailX, tailY) = FindTail(context.Map, position);
        context.Map[tailX, tailY] = (GameObject.Ground, Direction.None);
        return context.Map;
    }

    private static (int, int) FindTail((GameObject, Direction)[,] map, (int x, int y) position)
    {
        var (_, dir) = map[position.x, position.y];
        var (nx, ny) = dir switch
        {
            Direction.Up => (position.x + 1, position.y),
            Direction.Down => (position.x - 1, position.y),
            Direction.Left => (position.x, position.y + 1),
            Direction.Right => (position.x, position.y - 1),
            Direction.None => (position.x, position.y),
            _ => throw new ArgumentOutOfRangeException()
        };

        var (newX, newY, _) = FindNextPosition((position.x, position.y), (nx, ny), dir);
        var (obj, _) = map[newX, newY];

        if (obj == GameObject.Snake)
        {
            return FindTail(map, (newX, newY));
        }

        return position;
    }

    private (int, int) CreateFood((GameObject, Direction)[,] map)
    {
        var rnd = new Random();
        var x = rnd.Next(0, Rows);
        var y = rnd.Next(0, Cols);
        var (obj, _) = map[x, y];
        return obj == GameObject.Snake ? CreateFood(map) : (x, y);
    }
}