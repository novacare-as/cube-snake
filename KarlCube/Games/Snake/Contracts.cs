namespace KarlCube.Games.Snake;

public record GameStarted;
public record StatusTicked(int Score, int StepsLeft);
public record GameEnded(int Score);
