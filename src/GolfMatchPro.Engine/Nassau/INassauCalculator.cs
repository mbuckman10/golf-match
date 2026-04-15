namespace GolfMatchPro.Engine.Nassau;

public class NassauResult
{
    public int Front9Result { get; set; }  // +N = A wins, -N = B wins, 0 = halved
    public int Back9Result { get; set; }
    public int Overall18Result { get; set; }
    public int[] HoleByHoleStatus { get; set; } = new int[18]; // running status per hole
}

public interface INassauCalculator
{
    NassauResult CalculateMatchPlay(int[] netScoresA, int[] netScoresB);
    NassauResult CalculateMedalPlay(int[] netScoresA, int[] netScoresB);
}
