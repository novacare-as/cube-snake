using Gamepad;
using Iot.Device.LEDMatrix;
using KarlCube;

var mapping = PinMapping.MatrixBonnetMapping64;

var matrix = new RGBLedMatrix(mapping, 320, 64);
var game = new Game();
var cubeCtx = new CubeContext();
var gameCtx = game.CreateGameContext();
matrix.StartRendering();

var gamepad = new GamepadController();

gamepad.ButtonChanged += (object sender, ButtonEventArgs e) =>
{
    Console.WriteLine($"Button {e.Button} Pressed: {e.Pressed}");
};

gamepad.AxisChanged += (object sender, AxisEventArgs e) =>
{

    if (e.Axis is not (0 or 2)) return;
    switch (e.Value)
    {
        case 32767:
            cubeCtx.IsTurningLeft = true;
            return;
        case -32767:
            cubeCtx.IsTurningRight = true;
            return;
        default:
            cubeCtx.IsTurningRight = cubeCtx.IsTurningLeft = false;
            break;
    }
};

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
                    matrix.SetPixel(x, y, byte.MinValue, byte.MinValue, byte.MinValue);
                    break;
                case GameObject.Snake:
                    matrix.SetPixel(x, y, byte.MinValue, byte.MaxValue, byte.MinValue);
                    break;
                case GameObject.Food:
                    matrix.SetPixel(x, y, byte.MaxValue, byte.MinValue, byte.MinValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    Thread.Sleep(30);
    if (cubeCtx.IsTurningLeft)
    {
        gameCtx = game.Loop(gameCtx with { Direction = GetDirection.TurnLeft(gameCtx.Direction) });
        cubeCtx.IsTurningLeft = false;
    }
    else if (cubeCtx.IsTurningRight)
    {
        gameCtx = game.Loop(gameCtx with { Direction = GetDirection.TurnRight(gameCtx.Direction) });
        cubeCtx.IsTurningRight= false;
    }
    else
    {
        gameCtx = game.Loop(gameCtx);
    }
} while (!gameCtx.Dead);

matrix.StopRendering();
gamepad.Dispose();
