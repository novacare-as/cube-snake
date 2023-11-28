using CliWrap;
using Microsoft.Extensions.Logging;

namespace KarlCube;

public class ScreenSaver
{
    private readonly ILogger<ScreenSaver> _logger;
    private const string ImageViewer = "/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer";
    private const string Demo = "/home/pi/rpi-rgb-led-matrix/examples-api-use/demo";
    private bool IsPlayingGame; 


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

    private readonly ScreenSaverCommand _defaultScreenSaverCommand = new(ImageViewer, new []
    {
        "/home/pi/workshop/cube-snake/KarlCube/images/martin_snake_5_sides.gif",
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
        })
    };
    private readonly IEnumerable<ScreenSaverCommand> _christmasScreenSaverCommands = new[]
    {
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/campfire.gif",
            "--led-brightness=50"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/grinch.gif",
            "--led-brightness=30"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/merry-christmas.gif",
            "--led-brightness=80"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/parrot.gif",
            "--led-brightness=60"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/santa.gif",
            "--led-brightness=30"
        }),
        new ScreenSaverCommand(ImageViewer, new []
        {
            "-D 100",
            "/home/pi/workshop/cube-snake/KarlCube/images/snowman.gif",
            "--led-brightness=70"
        }),
        new ScreenSaverCommand(Demo, new []
        {
            "-D7",
            "--led-brightness=60",
            "--led-chain=5"
        })
    };

    private IEnumerable<ScreenSaverCommand> GetTimeAppropriateScreenSavers()
    {
        var now = DateTime.Now;

        return now.Month > 10 ? _christmasScreenSaverCommands : _randomScreenSaverCommands;
    }

    public async Task StartCycle()
    {
        var displayDefault = true;
        IsPlayingGame = false;
        do
        {
            var rnd = new Random();
            var screensavers = GetTimeAppropriateScreenSavers();
            var command = displayDefault ? _defaultScreenSaverCommand : screensavers.ElementAt(rnd.Next(0, screensavers.Count()));
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
