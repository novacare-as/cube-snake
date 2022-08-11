using Gamepad;
using Iot.Device.LEDMatrix;

var mapping = PinMapping.MatrixBonnetMapping64;

var matrix = new RGBLedMatrix(mapping, 320, 64);
var game = new Game();
var gameCtx = game.CreateGameContext();
matrix.StartRendering();

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
    Thread.Sleep(50);
    if (Console.KeyAvailable)
    {
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.Q:
                Environment.Exit(55);
                break;
            case ConsoleKey.LeftArrow:
                gameCtx = game.Loop(gameCtx with { Direction = GetDirection.TurnLeft(gameCtx.Direction) });
                break;
            case ConsoleKey.RightArrow:
                gameCtx = game.Loop(gameCtx with { Direction = GetDirection.TurnRight(gameCtx.Direction) });
                break;
        }
    }
    else
    {
        gameCtx = game.Loop(gameCtx);
    }
} while (!gameCtx.Dead);

matrix.StopRendering();
