namespace KarlCube;

public class CubeContext
{
    public State State { get; set; }
    public bool IsTurningLeft { get; set; }
    public bool IsTurningRight { get; set; }
}

public enum State
{
    Idle,
    Playing
}