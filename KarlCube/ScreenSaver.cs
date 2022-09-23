using CliWrap;
using Microsoft.Extensions.Logging;

namespace KarlCube;

public class ScreenSaver
{
    private readonly ILogger<ScreenSaver> _logger;
    private const string ImageViewer = "/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer";
    private const string Demo = "/home/pi/rpi-rgb-led-matrix/examples-api-use/demo";
    private bool IsPlayingGame; 

    private readonly ScreenSaverCommand _defaultScreenSaverCommand = new(ImageViewer, new []
    {
        "/home/pi/workshop/cube-snake/KarlCube/images/martin_test03_5_sides.gif",
        "--led-brightness=80",
        "--led-chain=5"
    });
    private readonly IEnumerable<ScreenSaverCommand> _randomScreenSaverCommands = new[]
    {
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/this-is-fine.gif",
            "--led-brightness=30"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/circle.gif",
            "--led-brightness=50"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/illusioncolor.gif",
            "--led-brightness=30"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/matrix-code.gif",
            "--led-brightness=10"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/star-wars.gif",
            "--led-brightness=40"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/outline.gif"
        }),
        new ScreenSaverCommand(Demo, new []
        {
            "-D7",
            "--led-brightness=60",
            "--led-chain=5"
        }),
        new ScreenSaverCommand(Demo, new []
        {
            "-D4",
            "--led-brightness=40",
            "--led-chain=5"
        })
    };
    private CancellationTokenSource _cancellationTokenSource;

    private readonly string[] _defaultCliArgs = {
        "--led-rows=64",
        "--led-cols=64",
        "--led-gpio-mapping=adafruit-hat-pwm",
        "--led-slowdown-gpio=2",
        "--led-no-drop-privs"
    };

    public ScreenSaver(ILogger<ScreenSaver> logger)
    {
        _logger = logger;
    }

    public async Task StartCycle()
    {
        var displayDefault = true;
        IsPlayingGame = false;
        do
        {
            var rnd = new Random();
            var command = displayDefault ? _defaultScreenSaverCommand : _randomScreenSaverCommands.ElementAt(rnd.Next(0, _randomScreenSaverCommands.Count()));
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.CancelAfter(30_000);
            try {
                await Cli.Wrap(command.Command)
                    .WithArguments(command.Args.Concat(_defaultCliArgs))
                    .ExecuteAsync(_cancellationTokenSource.Token);
            } catch (OperationCanceledException){
            }

            displayDefault = !displayDefault;
        } while (!IsPlayingGame);
    }
    
    public void Dispose()
    {
        IsPlayingGame = true;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}

public record ScreenSaverCommand(string Command, string[] Args);
