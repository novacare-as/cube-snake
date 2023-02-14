namespace KarlCube.Games.Shared;

public static class GetDirection {
    public static Direction TurnLeft(Direction currentDirection)
        => currentDirection switch
        {
            Direction.Up => Direction.Left,
            Direction.Down => Direction.Right,
            Direction.Left => Direction.Down,
            Direction.Right => Direction.Up
        };
    
    public static Direction TurnRight(Direction currentDirection)
        => currentDirection switch
        {
            Direction.Up => Direction.Right,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
            Direction.Right => Direction.Down
        };
    
    public static (int, int, Direction) FindNextPosition((int x, int y) oldPosition, (int newX, int newY) newPosition, Direction direction)
    {
        const int matrix = 64;
        var (x, y) = oldPosition;
        var (newX, newY) = newPosition;

        if (newX == matrix*5 && x == matrix*5-1)
        {
            return (matrix, newY, direction);
        }
        if (newX == matrix-1 && x == matrix)
        {
            return (matrix*5-1, newY, direction);
        }

        if (newY is -1 // Might go to top or to side 96-128 if already on top
            || (y == matrix-1 && newY == matrix) // From top and goes over the edge to side 32-64
            || newX is -1 or matrix) // From top to 128-160 (if x is 32) or 64-96 (if x is -1)
        {
            var side = (int) Math.Floor((decimal) (x / matrix));
            if (side is 0)
            {
                switch (newY)
                {
                    case -1:
                        return (matrix*3 + Math.Abs(matrix-1 - newX), 0, Direction.Right);
                    case matrix:
                        return (matrix*1 + newX, 0, Direction.Right);
                }
                switch (newX)
                {
                    case -1:
                        return (matrix*4 + newY, 0, Direction.Right);
                    case matrix:
                        return (matrix*2 + Math.Abs(matrix-1 - newY), 0, Direction.Right);
                }
            }

            if (side is 1)
            {
                if (newY == -1)
                {
                    return (newX - matrix, matrix-1, Direction.Left);
                }
            }
            
            if (side is 3)
            {
                if (newY == -1)
                {
                    return (Math.Abs(matrix-1 - (newX - matrix*3)), 0, Direction.Right);
                }
            }
            
            if (side is 2)
            {
                if (newY == -1)
                {
                    return (matrix-1, Math.Abs(matrix-1 - (newX - matrix*2)), Direction.Up);
                }
            }
            
            if (side is 4)
            {
                if (newY == -1)
                {
                    return (0, newX - matrix*4, Direction.Down);
                }
            }
            
        }

        return (newX, newY, direction);
    }
}