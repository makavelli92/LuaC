using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class Worker
    {
        public static void StartStrategy(Bars bars)
        {
            int lastIndex = 0;
            if (bars.MovingAverage != null)
                lastIndex = bars.MovingAverage.Count - 1;
            if (bars.StepPrice == 0)
                CalculateStepPrice(bars);

            if (bars.MovingAverage != null && bars.MovingAverage.Count > 0)
                DeleteLastData(bars);

            MovingAverageCalculate(bars);
            FractalCalculate(bars, lastIndex);

            Strategy.FindSignal(bars, EventSignal);
        }
        static void CalculateStepPrice(Bars bars)
        {
            decimal stepPrice = 0;
            List<decimal> temp = new List<decimal>()
            {
            (decimal)10, (decimal)1, (decimal)0.5, (decimal)0.1, (decimal)0.05, (decimal)0.01, (decimal)0.005, (decimal)0.00005, (decimal)0.00001
            };

            foreach (decimal i in temp)
            {

                for (int bar = 0; bar < bars.Count; bar++)
                {
                    if (((decimal)bars.High[bar] / i) - (int)((decimal)bars.High[bar] / i) != 0)
                        break;
                    if (bar == bars.Count - 1)
                        stepPrice = i;
                }
                if (stepPrice != 0)
                    break;
            }
            bars.StepPrice = Convert.ToDouble(stepPrice);
        }

        static void EventSignal(object e, string str)
        {
            Console.WriteLine("{0} - {1}\n", str, DateTime.Now);
        }

        public static void DeleteLastData(Bars bars)
        {
            if (bars.MovingAverage.Last() != 0)
                bars.MovingAverage.RemoveAt(bars.MovingAverage.Count - 1);
        }

        public static void FractalCalculate(Bars bars, int lastIndex)
        {
            if (bars.indexFractalHigh == null)
                bars.indexFractalHigh = new List<int>();
            if (bars.indexFractalsLow == null)
                bars.indexFractalsLow = new List<int>();

            if (bars.indexFractalHigh.Count > 0 || bars.indexFractalsLow.Count > 0)
            {
                for (int i = lastIndex; i > lastIndex - bars.sdvig; i--)
                {
                    bars.indexFractalHigh.RemoveAll(x => x == i);
                    bars.indexFractalsLow.RemoveAll(x => x == i);
                }
            }
            for (int i = (bars.indexFractalHigh.Count == 0 && bars.indexFractalsLow.Count == 0) ? bars.Count - 1 - bars.periodStrategy - (int)bars.fractalPeriod : lastIndex - bars.sdvig; i < bars.Count; i++)
            {
                int fractalUp = FindFractalHigh(i, bars.fractalPeriod, bars.High);
                int fractalDown = FindFractalLow(i, bars.fractalPeriod, bars.Low);

                if (fractalUp != -1)
                {
                    if (!bars.indexFractalHigh.Contains(fractalUp))
                        bars.indexFractalHigh.Add(fractalUp);

                    for (int j = fractalUp - bars.sdvig >= 0 ? fractalUp - bars.sdvig : 0; j < fractalUp; j++)
                    {
                        if (bars.High[j] > /*bars.MovingAverage[j + bars.sdvig]) ;*/(bars.MovingAverage.Count - 1 >= j + bars.sdvig? bars.MovingAverage[j + bars.sdvig]: bars.MovingAverage.Last()))
                        {
                            if (!bars.indexFractalHigh.Contains(j))
                                bars.indexFractalHigh.Add(j);
                        }

                    }
                    for (int j = fractalUp + bars.sdvig <= bars.Count - 1 ? fractalUp + bars.sdvig : bars.Count - 1; j > fractalUp; j--)
                    {
                        if (bars.High[j] > bars.MovingAverage[j - bars.sdvig])
                        {
                            if (!bars.indexFractalHigh.Contains(j))
                                bars.indexFractalHigh.Add(j);
                        }
                    }
                }
                if (fractalDown != -1)
                {
                    if (!bars.indexFractalsLow.Contains(fractalDown))
                        bars.indexFractalsLow.Add(fractalDown);

                    for (int j = fractalDown - bars.sdvig >= 0 ? fractalDown - bars.sdvig : 0; j < fractalDown; j++)
                    {
                        if (bars.Low[j] < /*bars.MovingAverage[j + bars.sdvig]*/ (bars.MovingAverage.Count - 1 >= j + bars.sdvig? bars.MovingAverage[j + bars.sdvig]:bars.MovingAverage.Last()))
                        {
                            if (!bars.indexFractalsLow.Contains(j))
                                bars.indexFractalsLow.Add(j);
                        }
                    }

                    for (int j = fractalDown + bars.sdvig <= bars.Count - 1 ? fractalDown + bars.sdvig : bars.Count - 1; j > fractalDown; j--)
                    {
                        if (bars.Low[j] < bars.MovingAverage[j - bars.sdvig])
                        {
                            if (!bars.indexFractalsLow.Contains(j))
                                bars.indexFractalsLow.Add(j);
                        }
                    }
                }
            }
            bars.indexFractalHigh.Sort();
            //foreach (int i in bars.indexFractalHigh)
            //{
            //    Console.WriteLine("High" + bars.Time[i]);
            //}
            bars.indexFractalsLow.Sort();
            //foreach (int i in bars.indexFractalsLow)
            //{
            //    Console.WriteLine("Low" + bars.Time[i]);
            //}

            //for (int i = bars.MovingAverage.Count - 201; i < bars.MovingAverage.Count; i++)
            //{
            //    Console.WriteLine("Low" + bars.MovingAverage[i]);
            //}
            ShowData(bars);
        }

        public static void ShowData(Bars bars)
        {
            Console.WriteLine(bars.LastTime);

            // Console.WriteLine("Count " +  bars.indexFractalsLow.Count);
            // Console.WriteLine("Last " + bars.Time[bars.indexFractalsLow.Last()]);
        }

        public static int FindFractalHigh(int i, double period, List<double> high)
        {
            int P = (int)Math.Floor(period / 2) * 2 + 1;
            if (i >= P)
            {
                int s = (int)(i - P + 1 + (int)Math.Floor(period / 2));

                double val_h = 0;
                for (int j = i - P + 1; j <= i; j++)
                {
                    if (high[j] > val_h)
                        val_h = high[j];
                }
                double h = high[s];
                if (val_h == h)
                    return s;
            }
            return -1;
        }
        public static int FindFractalLow(int i, double period, List<double> low)
        {
            int P = (int)Math.Floor(period / 2) * 2 + 1;
            if (i >= P)
            {
                int s = (int)(i - P + 1 + (int)Math.Floor(period / 2));

                double val_l = low[i - P + 1];
                for (int j = i - P + 2; j <= i; j++)
                {
                    if (low[j] < val_l)
                        val_l = low[j];
                }
                double l = low[s];
                if (val_l == l)
                    return s;
            }
            return -1;
        }

        public static void MovingAverageCalculate(Bars bars)
        {
            if (bars.MovingAverage == null)
            {
                bars.MovingAverage = new List<double>();
                for (int i = 0; i < bars.Count - 1 - bars.periodStrategy - bars.periodMoving - 1; i++)
                    bars.MovingAverage.Add(0);
            }

            switch (bars.movingCalculateType)
            {
                case "EMA":
                    {
                        EmaCalculateMA(bars);
                        break;
                    }
                default:
                    EmaCalculateMA(bars);
                    break;
            }
        }

        public static void EmaCalculateMA(Bars bars)
        {
            if (bars.Count >= bars.periodMoving)
            {
                //if (bars.MovingAverage.Count == bars.Count - 1 - bars.periodStrategy - bars.periodMoving - 1 )
                if (bars.MovingAverage.All(x => x == 0))
                {
                    bars.MovingAverage.Add(GetBarsValue(bars, bars.Count - 1 - bars.periodStrategy - bars.periodMoving - 1, bars.movingType));
                }
                for (int i = bars.MovingAverage.Count; i < bars.Count; i++)
                {
                    bars.MovingAverage.Add((bars.MovingAverage[i - 1] * (bars.periodMoving - 1) + 2 * GetBarsValue(bars, i, bars.movingType)) / (bars.periodMoving + 1));
                }
            }
        }

        public static double GetBarsValue(Bars bars, int index, string type)
        {
            switch (type)
            {
                case "Close":
                    {
                        return bars.Close[index];
                    }
                case "High":
                    {
                        return bars.High[index];
                    }
                case "Low":
                    {
                        return bars.Low[index];
                    }
                case "Median":
                    {
                        return (GetBarsValue(bars, index, "High") + GetBarsValue(bars, index, "High")) / 2;
                    }
                default:
                    return 0;
            }
        }
    }
}