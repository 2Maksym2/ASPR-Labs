namespace JordanExceptions
{
    public class MinimalPositiveRatioFinder
    {
        public MinimalPositiveRatioFinder() { }


        public int FindMinPositiveRatio(double[,] MatrixA, int column)
        {

            double temp = double.MaxValue;
            double pivot = double.MaxValue;
            int row = -1;

            int rowsCount = MatrixA.GetLength(0) - 1;
            int colsCount = MatrixA.GetLength(1) - 1;


            for (int i = 0; i < rowsCount; i++)
            {

                if (MatrixA[i, column] != 0)
                {
                    pivot = MatrixA[i, colsCount] / MatrixA[i, column];

                    if (MatrixA[i, colsCount] == 0 && MatrixA[i, column] > 0)
                    {
                        row = i;
                        break;
                    }

                }



                if (pivot > 0 && pivot < temp)
                {
                    temp = pivot;
                    row = i;
                }
            }
            return row;
        }
    }
}