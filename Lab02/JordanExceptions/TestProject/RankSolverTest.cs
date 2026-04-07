using JordanExceptions;

namespace TestProject
{
    public class RankSolverTest
    {
        [Theory]
        [InlineData(new double[] { 1, 2, 3, 4, 2, 4, 6, 8}, 2, 4, 1)]
        [InlineData(new double[] { 1, 2, 3, 6, 5, 10, 4, 8}, 4, 2, 1)]
        [InlineData(new double[] { 6, 2, 5, -3, 4, -1, 1, 4, 3}, 3, 3, 3)]
        [InlineData(new double[] { 1, 2, 3, 4, -2, 5, -1, 3, 2, 4, 6, 8, -1, 9, 2, 7}, 4, 4, 3)]
        [InlineData(new double[] { 2, 5, 4, -3, 1, -2, -1, 6 ,2}, 3, 3, 2)]
        [InlineData(new double[] { -1, 5, 4, -3, 1, -2, -1, 6, 2 }, 3, 3, 3)]
        [InlineData(new double[] {1,2,3,4,-2,5,-1,3,2,4,6,8,-1,7,2,7}, 4, 4, 2)]
        [InlineData(new double[] {1,2,3,4,-2,5,-1,3,2,4,7,8,-1,9,2,7}, 4, 4, 4)]
        public void GetSolution(double[] Matrix, int r, int c, int expected)
        {
            var _protocol = new SaveProtocol();

            double[,] inputMatrix = new double[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    inputMatrix[i, j] = Matrix[i * c + j];

            var _jordan = new JordanMethod();
            var _rank = new RankSolver(_protocol, _jordan);
            int rank = 0;


            rank = _rank.GetSolution(inputMatrix, r, c);

            
            Assert.Equal(expected, rank);
        }
    }
}