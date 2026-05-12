using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JordanExceptions
{
    public class SimulationRow
    {
        public int Id { get; set; }
        public double RandA { get; set; } 
        public string StrategyA { get; set; } 
        public double RandB { get; set; }     
        public string StrategyB { get; set; } 
        public double WinA { get; set; }      
        public double TotalWinA { get; set; } 
        public double AvgWinA { get; set; }   
    }
}
