namespace JordanExceptions
{
    public interface ISaveProtocol
    {
        void FileCleaner();
        void StepSave(string res);
        void InputMatrix(double[,] b);
        void ResultSave(int stepNumber, double[,] x, double[,] a);
        void InvertMatrixSave(double[,] b);
        void RankSave(int r, double[,] x, double[,] a);
        void RankResultSave(int r, double[,] x);


    }
}