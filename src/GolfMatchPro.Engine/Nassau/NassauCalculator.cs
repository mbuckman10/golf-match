namespace GolfMatchPro.Engine.Nassau;

public class NassauCalculator : INassauCalculator
{
    public NassauResult CalculateMatchPlay(int[] netScoresA, int[] netScoresB)
    {
        ValidateScores(netScoresA, netScoresB);

        var result = new NassauResult();
        int runningStatus = 0;
        int front9Status = 0;
        int back9Status = 0;

        for (int i = 0; i < 18; i++)
        {
            if (netScoresA[i] == 0 || netScoresB[i] == 0)
            {
                // Hole not yet played — carry forward
                result.HoleByHoleStatus[i] = runningStatus;
                continue;
            }

            if (netScoresA[i] < netScoresB[i])
                runningStatus++;
            else if (netScoresA[i] > netScoresB[i])
                runningStatus--;
            // else tie — no change

            result.HoleByHoleStatus[i] = runningStatus;

            // Track front/back independently
            if (i < 9)
            {
                if (netScoresA[i] < netScoresB[i]) front9Status++;
                else if (netScoresA[i] > netScoresB[i]) front9Status--;
            }
            else
            {
                if (netScoresA[i] < netScoresB[i]) back9Status++;
                else if (netScoresA[i] > netScoresB[i]) back9Status--;
            }
        }

        result.Front9Result = front9Status;
        result.Back9Result = back9Status;
        result.Overall18Result = runningStatus;

        return result;
    }

    public NassauResult CalculateMedalPlay(int[] netScoresA, int[] netScoresB)
    {
        ValidateScores(netScoresA, netScoresB);

        var result = new NassauResult();

        int frontA = 0, frontB = 0, backA = 0, backB = 0;
        int runningA = 0, runningB = 0;

        for (int i = 0; i < 18; i++)
        {
            runningA += netScoresA[i];
            runningB += netScoresB[i];
            // Running status: positive = A ahead (fewer strokes)
            result.HoleByHoleStatus[i] = runningB - runningA;

            if (i < 9)
            {
                frontA += netScoresA[i];
                frontB += netScoresB[i];
            }
            else
            {
                backA += netScoresA[i];
                backB += netScoresB[i];
            }
        }

        // Positive = A wins (fewer strokes), Negative = B wins
        result.Front9Result = frontB - frontA;
        result.Back9Result = backB - backA;
        result.Overall18Result = (frontB + backB) - (frontA + backA);

        return result;
    }

    private static void ValidateScores(int[] a, int[] b)
    {
        if (a.Length != 18 || b.Length != 18)
            throw new ArgumentException("Must provide exactly 18 hole scores.");
    }
}
