namespace Cube.Contracts;

public record GameStarted;
public record StatusTicked(int Score, int StepsLeft);
public record GameEnded(int Score);
