using CliWrap;
using Gamepad;
using MassTransit;
using rpi_rgb_led_matrix_sharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Cube.Contracts;
using Color = rpi_rgb_led_matrix_sharp.Color;
using System.IO;

namespace KarlCube;

public class GameHostedService : IHostedService
{
    private readonly ILogger<GameHostedService> _logger;
    private readonly IBus _bus;
    private readonly Game _game;
    private readonly CubeContext _cubeCtx;
    private readonly ScreenSaver _screenSaver;
    private GamepadController Gamepad {get;set;} 
    private GameContext _gameCtx;

    public GameHostedService(
        ILogger<GameHostedService> logger,
        IBus bus,
        ScreenSaver screenSaver)
    {
        _logger = logger;
        _bus = bus;
        _cubeCtx = new CubeContext();
        _screenSaver = screenSaver;
        _game = new Game();
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ReconnectGamepad(cancellationToken);
        Task.Run(_screenSaver.StartCycle, cancellationToken);
        ReconnectGamepadLoop(cancellationToken);
        return Task.CompletedTask;
    }
    
    private void ReconnectGamepad(CancellationToken cancellationToken)
    {
        try
        {
            Gamepad = new GamepadController();

            Gamepad.AxisChanged += (_, e) =>
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
            Gamepad.ButtonChanged += (_, e) =>
            {
                if (e.Button == 9 && e.Pressed && _cubeCtx.State == State.Idle)
                {
                    Task.Run(PlayGame, cancellationToken);
                }
            };
            
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning("Controller not found... Retrying in 5 sec");
            cancellationToken.WaitHandle.WaitOne(5000);
            if (cancellationToken.IsCancellationRequested) return;
            ReconnectGamepad(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _screenSaver.Dispose();
        Gamepad.Dispose();
        return Task.CompletedTask;
    }

    private async Task PlayGame()
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
                "--led-slowdown-gpio=3",
                "--led-brightness=20",
                "--led-no-drop-privs"
            }).ExecuteAsync();

        var matrix = new RGBLedMatrix(new RGBLedMatrixOptions
        {
            Cols = 64,
            Rows = 64,
            ChainLength = 5,
            HardwareMapping = "adafruit-hat-pwm",
            Brightness = 50,
            PwmDitherBits = 1,
            PwmLsbNanoseconds = 50,
            PwmBits = 7,
            GpioSlowdown = 3,
            DropPrivileges = false
        });
        
        var canvas = matrix.CreateOffscreenCanvas();
        _gameCtx = _game.CreateGameContext();
        
        Task.Run(RunStatusTick);
        do
        {
            for (var x = 0; x < _gameCtx.Map.GetLength(0); x++)
            {
                for (var y = 0; y < _gameCtx.Map.GetLength(1); y++)
                {
                    var (obj, _) = _gameCtx.Map[x, y];
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
            if (_cubeCtx.IsTurningLeft)
            {
                _gameCtx = _game.Loop(_gameCtx with { Direction = GetDirection.TurnLeft(_gameCtx.Direction) });
                _cubeCtx.IsTurningLeft = false;
            }
            else if (_cubeCtx.IsTurningRight)
            {
                _gameCtx = _game.Loop(_gameCtx with { Direction = GetDirection.TurnRight(_gameCtx.Direction) });
                _cubeCtx.IsTurningRight= false;
            }
            else
            {
                _gameCtx = _game.Loop(_gameCtx);
            }
            canvas = matrix.SwapOnVsync(canvas);
        } while (!_gameCtx.Dead);
        
        canvas.Clear();
        matrix.Dispose();
        
        await _bus.Publish(new GameEnded(_gameCtx.Score));

        await Cli.Wrap("/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer")
            .WithArguments(new[]
            {
                "-l 2",
                "-D 100",
                "/home/pi/workshop/cube-snake/KarlCube/images/gameover.gif",
                "--led-rows=64",
                "--led-cols=64",
                "--led-gpio-mapping=adafruit-hat-pwm",
                "--led-slowdown-gpio=3",
                "--led-brightness=20",
                "--led-no-drop-privs"
            }).ExecuteAsync();

        _cubeCtx.State = State.Idle;
        Task.Run(_screenSaver.StartCycle);
    }

    private async Task RunStatusTick()
    {
        do
        {
            await _bus.Publish(new StatusTicked(_gameCtx.Score, _gameCtx.StepsLeft));
            await Task.Delay(1000);
        } while (!_gameCtx.Dead);
    }
    
    private void ReconnectGamepadLoop(CancellationToken cancellationToken)
    {
        do
        {
            cancellationToken.WaitHandle.WaitOne(30_000);
            if (_cubeCtx.State != State.Idle) continue;
            if (!File.Exists("/dev/input/js0"))
            {
                Gamepad.Dispose();
                ReconnectGamepad(cancellationToken);
            }
            
        } while (!cancellationToken.IsCancellationRequested);
    }
}
