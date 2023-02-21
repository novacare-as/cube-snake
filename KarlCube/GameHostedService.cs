using CliWrap;
using KarlCube.Games.Achtung;
using MassTransit;
using rpi_rgb_led_matrix_sharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Color = rpi_rgb_led_matrix_sharp.Color;
using KarlCube.Games.Shared;
using KarlCube.Games.Snake;
using SnakeGameContext = KarlCube.Games.Snake.GameContext;
using GameObject = KarlCube.Games.Snake.GameObject;

namespace KarlCube;

public class GameHostedService : IHostedService
{
    private readonly ILogger<GameHostedService> _logger;
    private readonly IBus _bus;
    private readonly SnakeGame _snakeGame;
    private SnakeGameContext _snakeGameCtx;
    private readonly AchtungGame _achtungGame;
    private AchtungGameContext _achtungGameCtx;
    private readonly CubeContext _cubeCtx;
    private readonly ScreenSaver _screenSaver;

    public GameHostedService(
        ILogger<GameHostedService> logger,
        IBus bus,
        ScreenSaver screenSaver)
    {
        _logger = logger;
        _bus = bus;
        _cubeCtx = new CubeContext();
        _screenSaver = screenSaver;
        _snakeGame = new SnakeGame();
        _achtungGame = new AchtungGame();
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ConfigureGamepads(cancellationToken);
        Task.Run(_screenSaver.StartCycle, cancellationToken);
        return Task.CompletedTask;
    }
    
    private void ConfigureGamepads(CancellationToken cancellationToken)
    {
        foreach (var player in _cubeCtx.Players)
        {
            player.Gamepad.AxisChanged += (_, e) =>
            {
                if (e.Axis is not (0 or 2)) return;
                switch (e.Value)
                {
                    case 32767:
                        player.IsTurningLeft = true;
                        return;
                    case -32767:
                        player.IsTurningRight = true;
                        return;
                    default:
                        player.IsTurningRight = player.IsTurningLeft = false;
                        break;
                }
            };
            
            player.Gamepad.ButtonChanged += (_, e) =>
            {
                switch (e.Button)
                {
                    case 9 when e.Pressed && _cubeCtx.State == State.Idle:
                        Task.Run(() => PlaySnake(player.Id), cancellationToken);
                        break;
                    case 8 when e.Pressed && _cubeCtx.State == State.Idle:
                        Task.Run(PlayAchtung, cancellationToken);
                        break;
                }
            };
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _screenSaver.Dispose();
        foreach (var player in _cubeCtx.Players)
        {
            player.Gamepad.Dispose();
        }
        return Task.CompletedTask;
    }

    private async Task PlaySnake(int playerId)
    {
        _cubeCtx.State = State.Playing;
        _screenSaver.Dispose();
        await _bus.Publish(new GameStarted());
        await Cli.Wrap("/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer")
            .WithArguments(new[]
            {
                "-l 1",
                "-D 100",
                "/home/pi/workshop/cube-snake/KarlCube/images/countdown.gif",
                "--led-rows=64",
                "--led-cols=64",
                "--led-gpio-mapping=adafruit-hat-pwm",
                "--led-slowdown-gpio=2",
                "--led-brightness=40",
                "--led-no-drop-privs"
            }).ExecuteAsync();

        var matrix = new RGBLedMatrix(new RGBLedMatrixOptions
        {
            Cols = 64,
            Rows = 64,
            ChainLength = 5,
            HardwareMapping = "adafruit-hat-pwm",
            Brightness = 80,
            GpioSlowdown = 2,
            DropPrivileges = false
        });
        
        var canvas = matrix.CreateOffscreenCanvas();
        _snakeGameCtx = _snakeGame.CreateGameContext();
        
        Task.Run(RunStatusTick);
        do
        {
            for (var x = 0; x < _snakeGameCtx.Map.GetLength(0); x++)
            {
                for (var y = 0; y < _snakeGameCtx.Map.GetLength(1); y++)
                {
                    var (obj, _) = _snakeGameCtx.Map[x, y];
                    switch (obj)
                    {
                        case GameObject.Ground:
                            canvas.SetPixel(x, y, new Color(0, 0, 0));
                            break;
                        case GameObject.Snake:
                            canvas.SetPixel(x, y, new Color(0, 255, 0));
                            break;
                        case GameObject.Food:
                            canvas.SetPixel(x, y, new Color(255, 0, 0));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            await Task.Delay(20);
            var player = _cubeCtx.GetActivePlayer(playerId);
            if (player.IsTurningLeft)
            {
                _snakeGameCtx = _snakeGame.Loop(_snakeGameCtx with { Direction = GetDirection.TurnLeft(_snakeGameCtx.Direction) });
                player.IsTurningLeft = false;
            }
            else if (player.IsTurningRight)
            {
                _snakeGameCtx = _snakeGame.Loop(_snakeGameCtx with { Direction = GetDirection.TurnRight(_snakeGameCtx.Direction) });
                player.IsTurningRight = false;
            }
            else
            {
                _snakeGameCtx = _snakeGame.Loop(_snakeGameCtx);
            }
            canvas = matrix.SwapOnVsync(canvas);
        } while (!_snakeGameCtx.Dead);
        
        canvas.Clear();
        matrix.Dispose();
        
        await _bus.Publish(new GameEnded(_snakeGameCtx.Score));

        await Cli.Wrap("/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer")
            .WithArguments(new[]
            {
                "-l 2",
                "-D 100",
                "/home/pi/workshop/cube-snake/KarlCube/images/gameover.gif",
                "--led-rows=64",
                "--led-cols=64",
                "--led-gpio-mapping=adafruit-hat-pwm",
                "--led-slowdown-gpio=2",
                "--led-brightness=50",
                "--led-no-drop-privs"
            }).ExecuteAsync();

        _cubeCtx.State = State.Idle;
        Task.Run(_screenSaver.StartCycle);
    }
    
    private async Task PlayAchtung()
    {
        _cubeCtx.State = State.Playing;
        _screenSaver.Dispose();
        await Cli.Wrap("/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer")
            .WithArguments(new[]
            {
                "-l 1",
                "-D 100",
                "/home/pi/workshop/cube-snake/KarlCube/images/countdown.gif",
                "--led-rows=64",
                "--led-cols=64",
                "--led-gpio-mapping=adafruit-hat-pwm",
                "--led-slowdown-gpio=2",
                "--led-brightness=40",
                "--led-no-drop-privs"
            }).ExecuteAsync();

        var matrix = new RGBLedMatrix(new RGBLedMatrixOptions
        {
            Cols = 64,
            Rows = 64,
            ChainLength = 5,
            HardwareMapping = "adafruit-hat-pwm",
            Brightness = 80,
            GpioSlowdown = 2,
            DropPrivileges = false
        });
        
        var canvas = matrix.CreateOffscreenCanvas();
        _achtungGameCtx = _achtungGame.CreateGameContext();
        
        do
        {
            foreach (var player in _achtungGameCtx.Players)
            {
                if (player.MakeGap == 0)
                {
                    var (x, y) = player.Position;
                    var (r, g, b) = player.Color;
                    canvas.SetPixel(x, y, new Color(r, g, b));
                }
                
                var gamepad = _cubeCtx.GetActivePlayer(player.Id);
                if (gamepad.IsTurningLeft)
                {
                    player.Direction = GetDirection.TurnLeft(player.Direction);
                    gamepad.IsTurningLeft = false;
                }
                else if (gamepad.IsTurningRight)
                {
                    player.Direction = GetDirection.TurnRight(player.Direction);
                    gamepad.IsTurningRight = false;
                }
            }

            _achtungGameCtx = _achtungGame.Loop(_achtungGameCtx);
            await Task.Delay(20);
            canvas = matrix.SwapOnVsync(canvas);
        } while (!_achtungGameCtx.Players.Any(p => p.Dead));
        
        canvas.Clear();
        matrix.Dispose();

        await Cli.Wrap("/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer")
            .WithArguments(new[]
            {
                "-l 2",
                "-D 100",
                "/home/pi/workshop/cube-snake/KarlCube/images/gameover.gif",
                "--led-rows=64",
                "--led-cols=64",
                "--led-gpio-mapping=adafruit-hat-pwm",
                "--led-slowdown-gpio=2",
                "--led-brightness=50",
                "--led-no-drop-privs"
            }).ExecuteAsync();

        _cubeCtx.State = State.Idle;
        Task.Run(_screenSaver.StartCycle);
    }

    private async Task RunStatusTick()
    {
        do
        {
            await _bus.Publish(new StatusTicked(_snakeGameCtx.Score, _snakeGameCtx.StepsLeft));
            await Task.Delay(1000);
        } while (!_snakeGameCtx.Dead);
    }
}
