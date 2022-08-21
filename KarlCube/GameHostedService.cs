using CliWrap;
using Gamepad;
using rpi_rgb_led_matrix_sharp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Color = rpi_rgb_led_matrix_sharp.Color;

namespace KarlCube;

public class GameHostedService : IHostedService
{
    private readonly ILogger<GameHostedService> _logger;
    private readonly RGBLedMatrix _matrix;
    private readonly Game _game;
    private readonly CubeContext _cubeCtx;
    private GamepadController _gamepad;

    public GameHostedService(ILogger<GameHostedService> logger)
    {
        _logger = logger;
        _matrix = new RGBLedMatrix(new RGBLedMatrixOptions
        {
            Cols = 64,
            Rows = 64,
            ChainLength = 5,
            HardwareMapping = "adafruit-hat-pwm",
            DisableHardwarePulsing = true,
            Brightness = 50,
            PwmDitherBits = 1,
            PwmLsbNanoseconds = 50,
            PwmBits = 7,
            GpioSlowdown = 3
        });
        _game = new Game();
        _cubeCtx = new CubeContext();
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //await ScreenSaver();
        ConnectGamepad(cancellationToken);
    }

    private static async Task ScreenSaver()
    {
        try
        {
            var clt = new CancellationTokenSource();
            clt.CancelAfter(10000);
            var task = Cli.Wrap("/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer")
                .WithArguments(new[]
                {
                    "/home/pi/workshop/cube-snake/KarlCube/images/this-is-fine.gif",
                    "--led-rows=64",
                    "--led-cols=64",
                    "--led-gpio-mapping=adafruit-hat-pwm",
                    "--led-no-hardware-pulse",
                    "--led-slowdown-gpio=4",
                    "--led-brightness=20"
                }).ExecuteAsync(clt.Token);

            Console.WriteLine($"{task.ProcessId}");
            await task;
        }
        catch (OperationCanceledException e)
        {
        }
    }

    private void ConnectGamepad(CancellationToken cancellationToken)
    {
        try
        {
            _gamepad = new GamepadController();
            _gamepad.ButtonChanged += (_, e) =>
            {
                if (e.Button == 9 && e.Pressed && _cubeCtx.State == State.Idle)
                {
                    Task.Run(PlayGame, cancellationToken);
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
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Controller not found... Retrying in 10 sec");
            cancellationToken.WaitHandle.WaitOne(20000);
            if (cancellationToken.IsCancellationRequested) return;
            ConnectGamepad(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _matrix.Dispose();
        _gamepad.Dispose();
        return Task.CompletedTask;
    }

    private Task PlayGame()
    {
        var canvas = _matrix.CreateOffscreenCanvas();
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
            canvas = _matrix.SwapOnVsync(canvas);
        } while (!gameCtx.Dead);
        
        canvas.Clear();
        _cubeCtx.State = State.Idle;
        return Task.CompletedTask;
    }
}