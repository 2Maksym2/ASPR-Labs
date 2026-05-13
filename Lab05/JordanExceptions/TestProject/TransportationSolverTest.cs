using JordanExceptions;

namespace TestProject;

public class TransportationSolverTest
{
    [Fact]
    public void LabExample_NorthWest_ThenPotentials_MatchesMethodics()
    {
        double[,] c =
        {
            { 6, 3, 2 },
            { 2, 1, 5 },
            { 3, 4, 1 }
        };
        var s = new[] { 30.0, 20, 50 };
        var d = new[] { 10.0, 65, 25 };

        var r = TransportationSolver.Solve(c, s, d, TransportationSolver.InitialMethod.NorthWestCorner);

        Assert.Equal(265, r.InitialCost, 0.5);
        Assert.Equal(225, r.OptimalCost, 0.5);
    }
}
