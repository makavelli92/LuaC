using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Strategy
    {
        public static void FindSignal(Bars bars, EventHandler<string> eventHandler)
        {
            if(bars.LastTime == new DateTime(2017, 11, 17, 23, 25, 00))
            {
                Console.WriteLine("Here");
            }
            int bar = bars.Count - 1;
            int bsy;
            if (FindModelRepeatLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Short")// && DateStart(bar)) //&& DefenitionAreaNearLevel(bar) == "Short" && ExceptionBars(bar) && shortTrade)
            {
                eventHandler(new object(), "Повторяющийся уровень: БСУ - " + bsy + " / БПУ1 - " + (bar - 1) + " / БПУ2 - " + bar + " / Уровень = " + bars.High[bsy] + " / Люфт = " + bars.High[bsy] * 0.9996 + " | Снятие заявки от люфта = " + ((bars.High[bsy] * 0.9996) * 0.996));
            }
            if (FindModelMirrorLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Short")// && DateStart(bar))// && DefenitionAreaNearLevel(bar) == "Short" && ExceptionBars(bar) && shortTrade)
            {
                eventHandler(new object(), "Зеркальный уровень: БСУ - " + bsy + " / БПУ1 - " + (bar - 1) + " / БПУ2 - " + bar + " / Уровень = " + bars.Low[bsy] + " / Люфт = " + bars.Low[bsy] * 0.9996 + " | Снятие заявки от люфта = " + ((bars.Low[bsy] * 0.9996) * 0.996));
            }
            if (AirLevel(bar, bars, out bsy) == "Short")// && DateStart(bar))//&& DefenitionAreaNearLevel(bar) == "Nothing" && ExceptionBars(bar) && shortTrade)
            {
                ChangeColorConsole(true);
                eventHandler(new object(), "Воздушный уровень: БСУ - " + bsy + " / БПУ1 - " + (bar - 1) + " / БПУ2 - " + bar + " / Уровень = " + bars.Low[bsy] + " / Люфт = " + bars.Low[bsy] * 0.9996 + " | Снятие заявки от люфта = " + ((bars.Low[bsy] * 0.9996) * 0.996));
                ChangeColorConsole(false);
            }
            if (FindModelRepeatLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Long")// && DateStart(bar))// && DefenitionAreaNearLevel(bar) == "Long" && ExceptionBars(bar)  && longTrade)
            {
                eventHandler(new object(), "Повторяющийся уровень: БСУ - " + bsy + " / БПУ1 - " + (bar - 1) + " / БПУ2 - " + bar + " / Уровень = " + bars.Low[bsy] + " / Люфт = " + bars.Low[bsy] * 1.0004 + " | Снятие заявки от люфта = " + ((bars.Low[bsy] * 1.0004) * 1.004));
            }
            if (FindModelMirrorLevel(bar, bars.indexFractalHigh, bars.indexFractalsLow, bars, out bsy) == "Long")// && DateStart(bar))// && DefenitionAreaNearLevel(bar) == "Long" && ExceptionBars(bar) && longTrade)
            {
                eventHandler(new object(), "Зеркальный уровень: БСУ - " + bsy + " / БПУ1 - " + (bar - 1) + " / БПУ2 - " + bar + " / Уровень = " + bars.High[bsy] + " / Люфт = " + bars.High[bsy] * 1.0004 + " | Снятие заявки от люфта = " + ((bars.High[bsy] * 1.0004) * 1.004));
            }
            if (AirLevel(bar, bars, out bsy) == "Long")//&& DateStart(bar))// && DefenitionAreaNearLevel(bar) == "Nothing" && ExceptionBars(bar) && longTrade)
            {
                ChangeColorConsole(true);
                eventHandler(new object(), "Воздушный уровень: БСУ - " + bsy + " / БПУ1 - " + (bar - 1) + " / БПУ2 - " + bar + " / Уровень = " + bars.High[bsy] + " / Люфт = " + bars.High[bsy] * 1.0004 + " | Снятие заявки от люфта = " + ((bars.High[bsy] * 1.0004) * 1.004));
                ChangeColorConsole(false);
            }
        }

        public static void ChangeColorConsole(bool change, ConsoleColor color = ConsoleColor.Red)
        {
            if(change)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ResetColor();
        }

        /// <summary>
        /// Метод для определния с какого бара искать БСУ, если в графике меньше 540 баров, то с 0, если больше, то текущий бар - 540 баров назад
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="periodStrategy"> Как глубоко будем искать БСУ</param>
        /// <returns></returns>
        public static int StartBarForFindBSY(int bar, int periodStrategy)             
        {
            if (bar - periodStrategy > 0)
                return bar - periodStrategy;
            return 0;
        }
        public static string AirLevel(int bar, Bars bars, out int bsy)
        {
            if (bar > 2 && bars.High[bar - 2] == bars.High[bar - 3] &&
                (bars.High[bar - 1] >= (bars.High[bar - 2] - (bars.High[bar - 2] / 2500)) && bars.High[bar - 1] <= bars.High[bar - 2]) &&
                (bars.High[bar] >= (bars.High[bar - 2] - (bars.High[bar - 2] / 2500)) && bars.High[bar] <= bars.High[bar - 2])
                )
            {
                bsy = bar - 2;
                return "Short";
            }
            else if (bar > 2 && bars.Low[bar - 2] == bars.Low[bar - 3] && 
                (bars.Low[bar - 1] <= (bars.Low[bar - 2] + (bars.Low[bar - 2] / 2500)) && bars.Low[bar - 1] >= bars.Low[bar - 2]) &&
                (bars.Low[bar] <= (bars.Low[bar - 2] + (bars.Low[bar - 2] / 2500)) && bars.Low[bar] >= bars.Low[bar - 2])
                )
            {
                bsy = bar - 2;
                return "Long";
            }
            bsy = 0;
            return "Nothing";
        }
        public static bool BSYAndPBY1MirrorForLong(int bar, List<int> listHighFractal, Bars bars, out int bsy) // Для зеркального уровня, позиция в лонг
        {
            int fine = StartBarForFindBSY(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.High[temp] == bars.Low[bar]*/ (bars.High[temp] <= bars.Low[bar] && (bars.High[temp] + bars.StepPrice * bars.StepCount) >= bars.Low[bar]) && listHighFractal.Contains(temp))     // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool BSYAndPBY1MirrorForShort(int bar, List<int> listLowFractal, Bars bars, out int bsy) // Для зеркального уровня, позиция в шорт
        {
            int fine = StartBarForFindBSY(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.Low[temp] == bars.High[bar]*/(bars.Low[temp] >= bars.High[bar] && (bars.High[temp] - bars.StepPrice * bars.StepCount) <= bars.High[bar]) && listLowFractal.Contains(temp))     // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool BSYAndBPY1High(int bar, List<int> listHighFractal, Bars bars, out int bsy)               // Повторяющийся уровень для шорта
        {
            int fine = StartBarForFindBSY(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.High[temp] == bars.High[bar]*/(bars.High[temp] >= bars.High[bar] && (bars.High[temp] - bars.StepPrice * bars.StepCount) <= bars.High[bar]) && listHighFractal.Contains(temp)) // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool BSYAndBPY1Low(int bar, List<int> listLowFractal, Bars bars, out int bsy)                // Повторяющийся уровень для лонга
        {
            int fine = StartBarForFindBSY(bar, bars.periodStrategy);
            for (int temp = bar - 1; temp >= fine; temp--)
            {
                if (/*bars.Low[temp] == bars.Low[bar]*/(bars.Low[temp] <= bars.Low[bar] && (bars.Low[temp] + bars.StepPrice * bars.StepCount) >= bars.Low[bar]) && listLowFractal.Contains(temp))      // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
                {
                    bsy = temp;
                    return true;
                }
            }
            bsy = 0;
            return false;
        }
        public static bool BPY1AndBPY2High(int bar, Bars bars)              // Подтверждение повторяющегося уровеня для шорта (БПУ2)
        {
            if (bars.High[bar] >= (bars.High[bar - 1] - (bars.High[bar - 1] / 2500)) && bars.High[bar] <= bars.High[bar - 1])
                return true;
            return false;
        }
        public static bool BPY1AndBPY2Low(int bar, Bars bars)               // Подтверждение повторяющегося уровеня для лонга (БПУ2)   
        {
            if (bars.Low[bar] <= (bars.Low[bar - 1] + (bars.Low[bar - 1] / 2500)) && bars.Low[bar] >= bars.Low[bar - 1])
                return true;
            return false;
        }
        public static string FindModelRepeatLevel(int bar, List<int> listHighFractal, List<int> listLowFractal, Bars bars, out int BSY)                // Начало анализа баров для поиска повторяющегося уровня 
        {
            if (BSYAndBPY1High(bar - 1, listHighFractal, bars, out BSY) && BPY1AndBPY2High(bar, bars))
            {
                return "Short";
            }
            if (BSYAndBPY1Low(bar - 1, listLowFractal, bars, out BSY) && BPY1AndBPY2Low(bar, bars))
            {
                return "Long";
            }
            return "Nothing";
        }
        public static string FindModelMirrorLevel(int bar, List<int> listHighFractal, List<int> listLowFractal, Bars bars, out int BSY)                // Начало анализа баров для поиска зеркального уровня 
        {
            if (BSYAndPBY1MirrorForShort(bar - 1, listLowFractal, bars, out BSY) && BPY1AndBPY2High(bar, bars))
                return "Short";
            if (BSYAndPBY1MirrorForLong(bar - 1, listHighFractal, bars, out BSY) && BPY1AndBPY2Low(bar, bars))
                return "Long";
            return "Nothing";
        }
    }
}
