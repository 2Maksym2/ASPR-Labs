namespace JordanExceptions
{
    public interface ISaveProtocol
    {
        void FileCleaner();
        void StepSave(string res);


        void InputTableSave(double[,] matrix, string[] rows, string[] columns);
        void InputMatrix(double[,] b);

        void SaveSectionHeader(string message);
        void SaveStepHeader(string rowName, string colName, string title = "");
        void SaveTable(double[,] matrix, string[] rows, string[] columns);


        void ResultSimplexSave(double[] x);
        void ResultSimplexSave(double[] x, double zValue);
        void ResultSave(int stepNumber, double[,] x, double[,] a);

        void InvertMatrixSave(double[,] b);
        void RankSave(int r, double[,] x, double[,] a);
        void RankResultSave(int r, double[,] x);

    }
}