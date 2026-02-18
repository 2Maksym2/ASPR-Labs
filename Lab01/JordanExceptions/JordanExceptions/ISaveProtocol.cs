namespace JordanExceptions
{
    public interface ISaveProtocol
    {
        void FileCleaner();
        void StepSave(string res);
        void InputMatrix(double[,] b);
        void ResultSave(int stepNumber, double[,] x);
        void InvertMatrixSave(double[,] b);


    }
}