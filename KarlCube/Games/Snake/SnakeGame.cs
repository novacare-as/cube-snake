using KarlCube.Games.Shared;

namespace KarlCube.Games.Snake;

public class SnakeGame
{
    private const int Matrix = 64;
    private const int Rows = Matrix*5;
    private const int Cols = Matrix;

    public GameContext CreateGameContext()
    {
        var map = new (GameObject, Direction)[Rows,Cols];

        map[Matrix/2, Matrix/2] = (GameObject.Snake, Direction.Up);
        map[Matrix/2+1, Matrix/2] = (GameObject.Snake, Direction.Up);
        map[Matrix/2+2, Matrix/2] = (GameObject.Snake, Direction.Up);
        var (foodX, foodY) = CreateFood(map);
        map[foodX, foodY] = (GameObject.Food, Direction.None);
        return new GameContext
        {
            Map = map,
            Direction = Direction.Up,
            Position = (Matrix/2, Matrix/2),
            Score = 0,
            StepsLeft = 800,
            Dead = false,
            GrowBy = 0
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

        var (newX, newY, direction) = GetDirection.FindNextPosition((x, y), (nx, ny), context.Direction);
        
        try
        {
            var (obj, _) = context.Map[newX, newY];

            return obj switch
            {
                GameObject.Ground => context with
                {
                    Map = ModifyHitGround(context.Map, (newX, newY), direction, context.GrowBy),
                    Position = (newX, newY),
                    StepsLeft = context.StepsLeft - 1,
                    Direction = direction,
                    GrowBy = context.GrowBy > 0 ? context.GrowBy - 1 : 0
                },
                GameObject.Snake => context with
                {
                    Dead = true,
                    Position = (newX, newY),
                    Direction = direction
                },
                GameObject.Food => context with
                {
                    Map = ModifyHitFood(context.Map, (newX, newY), direction),
                    Position = (newX, newY),
                    StepsLeft = context.StepsLeft + 200,
                    Score = context.Score + 10,
                    GrowBy = context.GrowBy + 10,
                    Direction = direction
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception e)
        {
            return context with {Dead = true};
        }
    }
    
    private (GameObject, Direction)[,] ModifyHitFood((GameObject, Direction)[,] map, (int x, int y) position, Direction direction)
    {
        map[position.x, position.y] = (GameObject.Snake, direction);
        var (foodX, foodY) = CreateFood(map);
        map[foodX, foodY] = (GameObject.Food, Direction.None);
        return map;
    }

    private static (GameObject, Direction)[,] ModifyHitGround((GameObject, Direction)[,] map, (int x, int y) position, Direction direction, int growBy)
    {
        map[position.x, position.y] = (GameObject.Snake, direction);
        if (growBy == 0) {
            var (tailX, tailY) = FindTail(map, position, position);
            map[tailX, tailY] = (GameObject.Ground, Direction.None);
        }
        return map;
    }

    private static (int, int) FindTail((GameObject, Direction)[,] map, (int x, int y) position, (int x, int y) headposition)
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

        var (newX, newY, _) = GetDirection.FindNextPosition((position.x, position.y), (nx, ny), dir);
        var (obj, _) = map[newX, newY];

        if (newX == headposition.x && newY == headposition.y){
            throw new Exception("Chickening out. Avoiding recursive loop...");
        }

        if (obj == GameObject.Snake)
        {
            return FindTail(map, (newX, newY), headposition);
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
