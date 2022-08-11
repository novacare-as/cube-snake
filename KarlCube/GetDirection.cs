namespace KarlCube;

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
}