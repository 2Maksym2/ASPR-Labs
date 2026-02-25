namespace JordanExceptions
{
    public interface IJordanMethod
    {
        double[,] MatrixSolver(double[,] matrixA, double a, int r, int s);
    }
}