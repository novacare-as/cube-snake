using CliWrap;

namespace KarlCube;

public class ScreenSaver
{
    private readonly CubeContext _cubeContext;
    private const string ImageViewer = "/home/pi/rpi-rgb-led-matrix/utils/led-image-viewer";
    private const string Demo = "/home/pi/rpi-rgb-led-matrix/examples-api-use/demo";

    private readonly ScreenSaverCommand _defaultScreenSaverCommand = new(ImageViewer, new []
    {
        "/home/pi/workshop/cube-snake/KarlCube/images/martin_test02_5_sides.gif",
        "--led-brightness=10"
    });
    private readonly IEnumerable<ScreenSaverCommand> _randomScreenSaverCommands = new[]
    {
        new ScreenSaverCommand(ImageViewer, new []
        {
            "/home/pi/workshop/cube-snake/KarlCube/images/this-is-fine.gif",
            "--led-brightness=10"
        }),
        new ScreenSaverCommand(Demo, new []
        {
            "-D7",
            "--led-brightness=40",
            "--led-chain=5"
        }),
        new ScreenSaverCommand(Demo, new []
        {
            "-D4",
            "--led-brightness=20",
            "--led-chain=5"
        })
    };
    private CancellationTokenSource _cancellationTokenSource;

    private readonly string[] _defaultCliArgs = {
        "--led-rows=64",
        "--led-cols=64",
        "--led-gpio-mapping=adafruit-hat-pwm",
        "--led-slowdown-gpio=3",
        "--led-no-drop-privs"
    };

    public ScreenSaver(CubeContext cubeContext)
    {
        _cubeContext = cubeContext;
    }

    public async Task StartCycle()
    {
        var displayDefault = true;
        do
        {
            var rnd = new Random();
            var command = displayDefault ? _defaultScreenSaverCommand : _randomScreenSaverCommands.ElementAt(rnd.Next(0, _randomScreenSaverCommands.Count()));
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.CancelAfter(60000);
            await ExecuteScreenSaverCommand(command);
            displayDefault = !displayDefault;
        } while (_cubeContext.State == State.Idle);
    }
    
    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }

    private Task ExecuteScreenSaverCommand(ScreenSaverCommand screenSaverCommand)
    {
        return Cli.Wrap(screenSaverCommand.Command)
            .WithArguments(screenSaverCommand.Args.Concat(_defaultCliArgs))
            .ExecuteAsync(_cancellationTokenSource.Token);
    }
}

public record ScreenSaverCommand(string Command, string[] Args);