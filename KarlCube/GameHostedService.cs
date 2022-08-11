using Gamepad;
using Iot.Device.LEDMatrix;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KarlCube;

public class GameHostedService : IHostedService
{
    private readonly ILogger<GameHostedService> _logger;
    private readonly RGBLedMatrix _matrix;
    private readonly Game _game;
    private readonly CubeContext _cubeCtx;
    private readonly GamepadController _gamepad;

    public GameHostedService(ILogger<GameHostedService> logger)
    {
        _logger = logger;
        _matrix = new RGBLedMatrix(PinMapping.MatrixBonnetMapping64, 320, 64);
        _game = new Game();
        _cubeCtx = new CubeContext();
        _gamepad = new GamepadController();
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _gamepad.ButtonChanged += (_, e) =>
        {
            if (e.Button == 9 && e.Pressed && _cubeCtx.State == State.Idle)
            {
                PlayGame();
            }
        };

        _gamepad.AxisChanged += (_, e) =>
        {

            if (e.Axis is not (0 or 2)) return;
            switch (e.Value)
            {
                case 32767:
                    _cubeCtx.IsTurningLeft = true;
                    return;
                case -32767:
                    _cubeCtx.IsTurningRight = true;
                    return;
                default:
                    _cubeCtx.IsTurningRight = _cubeCtx.IsTurningLeft = false;
                    break;
            }
        };
        _matrix.StartRendering();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _matrix.StopRendering();
        _gamepad.Dispose();
        return Task.CompletedTask;
    }

    private void PlayGame()
    {
        _cubeCtx.State = State.Playing;
        var gameCtx = _game.CreateGameContext();
        do
        {
            for (var x = 0; x < gameCtx.Map.GetLength(0); x++)
            {
                for (var y = 0; y < gameCtx.Map.GetLength(1); y++)
                {
                    var (obj, _) = gameCtx.Map[x, y];
                    switch (obj)
                    {
                        case GameObject.Ground:
                            _matrix.SetPixel(x, y, byte.MinValue, byte.MinValue, byte.MinValue);
                            break;
                        case GameObject.Snake:
                            _matrix.SetPixel(x, y, byte.MinValue, byte.MaxValue, byte.MinValue);
                            break;
                        case GameObject.Food:
                            _matrix.SetPixel(x, y, byte.MaxValue, byte.MinValue, byte.MinValue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            Thread.Sleep(30);
            if (_cubeCtx.IsTurningLeft)
            {
                gameCtx = _game.Loop(gameCtx with { Direction = GetDirection.TurnLeft(gameCtx.Direction) });
                _cubeCtx.IsTurningLeft = false;
            }
            else if (_cubeCtx.IsTurningRight)
            {
                gameCtx = _game.Loop(gameCtx with { Direction = GetDirection.TurnRight(gameCtx.Direction) });
                _cubeCtx.IsTurningRight= false;
            }
            else
            {
                gameCtx = _game.Loop(gameCtx);
            }
        } while (!gameCtx.Dead);

        _cubeCtx.State = State.Idle;
    }
}