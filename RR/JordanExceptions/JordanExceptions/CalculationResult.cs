using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class CalculationResult
    {
        public double[,] OriginalMatrix { get; set; }
        public double[] P { get; set; }
        public double[] Q { get; set; }
        public double V { get; set; }
    }
}
