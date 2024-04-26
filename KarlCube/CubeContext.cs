using Gamepad;

namespace KarlCube;

public class CubeContext
{
    public State State { get; set; }

    public IEnumerable<Player> Players { get; } = new[]
    {
        new Player
        {
            Id = 1,
            Gamepad = new GamepadController()
        },
        new Player
        {
            Id = 2,
            Gamepad = new GamepadController("/dev/input/js1")
        }
    };

    public Player GetActivePlayer(int playerId) =>  Players.First(p => p.Id == playerId);
}

public class Player
{
    public int Id { get; init; }
    public bool IsTurningLeft { get; set; }
    public bool IsTurningRight { get; set; }
    public GamepadController Gamepad { get; init; }
}

public enum State
{
    Idle,
    Playing
}