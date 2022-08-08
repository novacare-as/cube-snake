using Iot.Device.LEDMatrix;
using KarlCube;

var mapping = PinMapping.MatrixBonnetMapping32;

var matrix = new RGBLedMatrix(mapping, 160, 32);
var game = new Game();
var gameCtx = game.CreateGameContext();
matrix.StartRendering();

do
{
    for (var row = 0; row < 32; row++)
    {
        for (int column = 0; column < 32; column++)
        {
            var color = Countdown.Three[row, column];
            if (column > 0)
            {
                matrix.SetPixel(row, column, (byte)((color / 0xffFF) & 0xff), (byte)((color / 0xff) & 0xff), (byte)(color & 0xff));
            }
        }
    }
    Thread.Sleep(2000);

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

                case GameObject.Graphics:
                    matrix.SetPixel(x, y, byte.MaxValue, byte.MaxValue, byte.MaxValue);
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