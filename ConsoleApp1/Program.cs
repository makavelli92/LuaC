using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private delegate void TB(List<Data> listBars, String[] substrings, StreamWriter SW_Command, StreamReader SR_FlagCommand, StreamWriter SW_FlagCommand);

        private static Mutex mutexCmd;
        private static Mutex mutexDat;

        private const string mutexCommand = "MutexForCommand";
        private const string mutexData = "MutexForData";

        private static List<Data> listBars;

        private static string GetCommandString(Security security, TimeFrame timeFrame)
        {
            return "TQBR" + ';' + security.ToString() + ';' + (int)timeFrame + ';' + 0;
        }

        private static string GetCommandString(Futures security, TimeFrame timeFrame)
        {
            return "SPBFUT" + ';' + security.ToString() + ';' + (int)timeFrame + ';' + 0;
        }

        static void Main(string[] args)
        {
            listBars = new List<Data>();

            try
            {
                mutexCmd = Mutex.OpenExisting(mutexCommand);
            }
            catch
            {
                mutexCmd = new Mutex(false, mutexCommand);
            }
            try
            {
                mutexDat = Mutex.OpenExisting(mutexData);
                Console.WriteLine("Open");

            }
            catch
            {
                mutexDat = new Mutex(false, mutexData);
                Console.WriteLine("Create");

            }
            // Создаст, или подключится к уже созданной памяти с таким именем
            MemoryMappedFile Memory = MemoryMappedFile.CreateOrOpen("Memory", 200000, MemoryMappedFileAccess.ReadWrite);
            MemoryMappedFile Flag = MemoryMappedFile.CreateOrOpen("Flag", 1, MemoryMappedFileAccess.ReadWrite);
            MemoryMappedFile Command = MemoryMappedFile.CreateOrOpen("Command", 128, MemoryMappedFileAccess.ReadWrite);
            MemoryMappedFile FlagCommand = MemoryMappedFile.CreateOrOpen("FlagCommand", 1, MemoryMappedFileAccess.ReadWrite);
            // Создает поток для чтения
            StreamReader SR_Memory = new StreamReader(Memory.CreateViewStream(), System.Text.Encoding.Default);
            // Создает поток для записи
            StreamWriter SW_Memory = new StreamWriter(Memory.CreateViewStream(), System.Text.Encoding.Default);

            StreamReader SR_Flag = new StreamReader(Flag.CreateViewStream(), System.Text.Encoding.Default);
            StreamWriter SW_Flag = new StreamWriter(Flag.CreateViewStream(), System.Text.Encoding.Default);

            StreamReader SR_Command = new StreamReader(Command.CreateViewStream(), System.Text.Encoding.Default);
            StreamWriter SW_Command = new StreamWriter(Command.CreateViewStream(), System.Text.Encoding.Default);

            StreamReader SR_FlagCommand = new StreamReader(FlagCommand.CreateViewStream(), System.Text.Encoding.Default);
            StreamWriter SW_FlagCommand = new StreamWriter(FlagCommand.CreateViewStream(), System.Text.Encoding.Default);
            
            string Msg = "";
            string flag = "";

            SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
            SW_Flag.Write("o");
            SW_Flag.Flush();

            SW_FlagCommand.BaseStream.Seek(0, SeekOrigin.Begin);
            SW_FlagCommand.Write("o");
            SW_FlagCommand.Flush();

            Task.Run(() => {
                Program.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, GetCommandString(Futures.SRZ7, TimeFrame.INTERVAL_TICK) + ";" + GetCommandString(Futures.SiZ7, TimeFrame.INTERVAL_TICK));
                Program.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, GetCommandString(Futures.RIZ7, TimeFrame.INTERVAL_TICK));
            });
           
            // Цикл работает пока Run == true
            int m = 0;
            while (true)
            {
                do
                {
                    SR_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                    flag = SR_Flag.ReadToEnd().Trim('\0', '\r', '\n');
                }
                while (flag == "o" || flag == "c");
                
                SR_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                flag = SR_Flag.ReadToEnd().Trim('\0', '\r', '\n');
                if(flag != "c" && (flag == "p" || flag == "l"))
                {
                    mutexDat.WaitOne();
                    ++m;
                    Console.WriteLine("Get data from c++");
                    if (flag == "p")
                    {
                        Console.WriteLine("Get data == p");
                        string str;
                        do
                        {
                            SR_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                            flag = SR_Flag.ReadToEnd().Trim('\0', '\r', '\n');
                            if (flag != "e")
                            {
                                // Встает в начало потока для чтения
                                SR_Memory.BaseStream.Seek(0, SeekOrigin.Begin);
                                // Считывает данные из потока памяти, обрезая ненужные байты
                                str = SR_Memory.ReadToEnd().Trim('\0', '\r', '\n');
                                Msg += str;
                                Console.WriteLine(Msg.Length);
                                // Встает в начало потока для записи
                                SW_Memory.BaseStream.Seek(0, SeekOrigin.Begin);
                                // Очищает память, заполняя "нулевыми байтами"
                                for (int i = 0; i < 200000; i++)
                                {
                                    SW_Memory.Write("\0");
                                }
                                SW_Memory.Flush();
                                
                                if(flag == "l")
                                {
                                    //SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                                    //SW_Flag.Write("e");
                                    //SW_Flag.Flush();
                                }
                                else if (flag == "p")
                                {
                                    Console.WriteLine("Write e");
                                    SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                                    SW_Flag.Write("e");
                                    SW_Flag.Flush();
                                    SR_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                                    flag = SR_Flag.ReadToEnd().Trim('\0', '\r', '\n');
                                    while (flag == "e")
                                    {
                                        mutexDat.ReleaseMutex();
                                        --m;
                                        
                                        SR_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                                        flag = SR_Flag.ReadToEnd().Trim('\0', '\r', '\n');
                                        mutexDat.WaitOne();
                                        ++m;
                                        Thread.Sleep(100);
                                    }
                                }
                            }
                           // Thread.Sleep(10);
                        }
                        while (flag != "l");
                    }
                    if(flag == "l")
                    {
                        SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                        SW_Flag.Write("c");
                        SW_Flag.Flush();
                        // Встает в начало потока для чтения
                        SR_Memory.BaseStream.Seek(0, SeekOrigin.Begin);
                        // Считывает данные из потока памяти, обрезая ненужные байты
                        Msg += SR_Memory.ReadToEnd().Trim('\0', '\r', '\n');
                        
                    }

                    String[] substrings = Msg.Split(';');

                    if (Msg != "" && substrings.Count() > 3)
                    {
                        // Потокобезопасно выводит сообщение в текстовое поле
                        // TB delegateShow = Program.ShowText;
                        TB delegateShow = Program.AddData;
                        delegateShow.BeginInvoke(listBars, substrings, SW_Command, SR_FlagCommand, SW_FlagCommand, null, null);

                        Msg = String.Empty;

                        SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                        SW_Flag.Write("c");
                        SW_Flag.Flush();

                        // Встает в начало потока для записи
                        SW_Memory.BaseStream.Seek(0, SeekOrigin.Begin);
                        // Очищает память, заполняя "нулевыми байтами"
                        for (int i = 0; i < 200000; i++)
                        {
                            SW_Memory.Write("\0");
                        }
                        // Очищает все буферы для SW_Memory и вызывает запись всех данных буфера в основной поток
                        SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                        SW_Flag.Write("o");
                        SW_Flag.Flush();
                        SW_Memory.Flush();
                    }
                    else
                    {
                        Data temp = listBars.FirstOrDefault(x => x.Name == substrings[0] && x.TimeFrame == Int32.Parse(substrings[1]));
                        Task.Run(() => {
                            Program.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';' + temp.Time.Count);
                        });

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';' + temp.Time.Count);
                        Console.ResetColor();

                        Msg = String.Empty;

                        SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                        SW_Flag.Write("c");
                        SW_Flag.Flush();

                        // Встает в начало потока для записи
                        SW_Memory.BaseStream.Seek(0, SeekOrigin.Begin);
                        // Очищает память, заполняя "нулевыми байтами"
                        for (int i = 0; i < 200000; i++)
                        {
                            SW_Memory.Write("\0");
                        }
                        // Очищает все буферы для SW_Memory и вызывает запись всех данных буфера в основной поток
                        SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
                        SW_Flag.Write("o");
                        SW_Flag.Flush();
                        SW_Memory.Flush();
                    }
                    mutexDat.ReleaseMutex();
                    --m;
                }
            }
            // По завершению цикла, закрывает все потоки и освобождает именованную память
            SR_Memory.Close();
            SW_Memory.Close();
            Memory.Dispose();


            Console.ReadLine();
        }

        public static void ShowText(string Msg)
        {
            // Добавляет к сообщению символ перехода на новую строку
            // Console.WriteLine(Msg + Environment.NewLine);
            String[] substrings = Msg.Split(';');
            Console.WriteLine(Msg.Count());

        }
        public static void AddData(List<Data> listBars, String[] substrings, StreamWriter SW_Command, StreamReader SR_FlagCommand, StreamWriter SW_FlagCommand)
        {
            Data temp = listBars.FirstOrDefault(x => x.Name == substrings[0] && x.TimeFrame == Int32.Parse(substrings[1]));
            
            if(substrings[1] != "0")
            {
                Bars tmp = temp as Bars;
                for (int i = 2; i < substrings.Length - 1; i = i + 6)
                {
                    tmp.Time.Add(DateTime.Parse(substrings[i]));

                    tmp.Open.Add(Double.Parse(substrings[i + 1], CultureInfo.InvariantCulture));

                    tmp.High.Add(Double.Parse(substrings[i + 2], CultureInfo.InvariantCulture));

                    tmp.Low.Add(Double.Parse(substrings[i + 3], CultureInfo.InvariantCulture));

                    tmp.Close.Add(Double.Parse(substrings[i + 4], CultureInfo.InvariantCulture));

                    tmp.Volume.Add(Double.Parse(substrings[i + 5], CultureInfo.InvariantCulture));
                }

             //   Worker.StartStrategy(tmp);
            }
            else
            {
                if (substrings[2] == "0")
                {
                    temp.Count = Int32.Parse(substrings[3]);
                    Task.Run(() => {
                        Program.SetQUIKCommandData(SW_Command, SR_FlagCommand, SW_FlagCommand, temp.ClassCod + ';' + temp.Name + ';' + temp.TimeFrame + ';' + (temp.Count - 1));
                    });
                }
                else
                {
                    for (int i = 3; i < substrings.Length - 1; i = i + 3)
                    {
                        temp.Time.Add(DateTime.Parse(substrings[i]));

                        temp.Close.Add(Double.Parse(substrings[i + 1], CultureInfo.InvariantCulture));

                        temp.Volume.Add(Double.Parse(substrings[i + 2], CultureInfo.InvariantCulture));
                    }
                }
            }
            Console.WriteLine(temp.Name);
            if(temp.Time.Count > 0)
            {
                Console.WriteLine(temp.Time[temp.Time.Count() - 1]);
                Console.WriteLine(temp.Close[temp.Close.Count() - 1]);
                Console.WriteLine(temp.Volume[temp.Volume.Count() - 1]);
            }
        }

        public static void SetQUIKCommandData(StreamWriter SW_Command, StreamReader SR_FlagCommand, StreamWriter SW_FlagCommand, string Data = "")
        {
            
            //Если нужно отправить команду
            if (Data != "")
            {
                String[] substrings = Data.Split(';');

                for (int i = 0; i < substrings.Length - 1; i = i + 4)
                {
                    Data temp = listBars.FirstOrDefault(x => x.Name == substrings[i + 1] && x.TimeFrame == Int32.Parse(substrings[2]));
                    if (temp == null)
                    {
                        if (substrings[i + 2] == "0")
                            listBars.Add(new Ticks() { ClassCod = substrings[i], Name = substrings[i + 1], TimeFrame = Int32.Parse(substrings[i + 2]), Time = new List<DateTime>(), Close = new List<double>(), Volume = new List<double>() });
                        else
                            listBars.Add(new Bars() { ClassCod = substrings[i], Name = substrings[i + 1], TimeFrame = Int32.Parse(substrings[i + 2]), Time = new List<DateTime>(), Open = new List<double>(), High = new List<double>(), Low = new List<double>(), Close = new List<double>(), Volume = new List<double>() });
                    }
                }

                //Дополняет строку команды "нулевыми байтами" до нужной длины
                for (int i = Data.Length; i < 128; i++) Data += "\0";
            }
            else //Если нужно очистить память
            { //Заполняет строку для записи "нулевыми байтами"
                for (int i = 0; i < 128; i++) Data += "\0";
            }
            string flag = "";
            
            do
            {
                if (flag != "")
                    Thread.Sleep(10);
                SR_FlagCommand.BaseStream.Seek(0, SeekOrigin.Begin);
                flag = SR_FlagCommand.ReadToEnd().Trim('\0', '\r', '\n');
            }
            while (flag != "o");

            mutexCmd.WaitOne();

            SW_FlagCommand.BaseStream.Seek(0, SeekOrigin.Begin);
            SW_FlagCommand.Write("c");
            SW_FlagCommand.Flush();
            //Встает в начало

            SW_Command.BaseStream.Seek(0, SeekOrigin.Begin);
            //Записывает строку
            SW_Command.Write(Data);
            //Сохраняет изменения в памяти
            SW_Command.Flush();
            Console.WriteLine("Command send from c#");

            SW_FlagCommand.BaseStream.Seek(0, SeekOrigin.Begin);
            SW_FlagCommand.Write("r");
            SW_FlagCommand.Flush();

            mutexCmd.ReleaseMutex();
        }
    }
    public enum TimeFrame
    {
        INTERVAL_TICK = 0,      //   Тиковые данные
        INTERVAL_M1 = 1,        //  1 минута
        INTERVAL_M2 = 2,        //  2 минуты
        INTERVAL_M3 = 3,        //  3 минуты
        INTERVAL_M4 = 4,        //  4 минуты
        INTERVAL_M5 = 5,        //  5 минут
        INTERVAL_M6 = 6,        //  6 минут
        INTERVAL_M10 = 10,      //  10 минут
        INTERVAL_M15 = 15,      //  15 минут
        INTERVAL_M20 = 20,      //  20 минут
        INTERVAL_M30 = 30,      //   30 минут
        INTERVAL_H1 = 60,       //   1 час
        INTERVAL_H2 = 120,      //   2 часа
        INTERVAL_H4 = 240,      // 4 часа
        INTERVAL_D1 = 1440,     // 1 день
        INTERVAL_W1 = 10080,    //  1 неделя
        INTERVAL_MN1 = 23200,   //   1 месяц
    }

    public enum ClassCod
    {
        SPBFUT = 1,
        TQBR = 2
    }
    public enum Futures
    {
        GZZ7,
        SRZ7,
        EuZ7,
        GDZ7,
        RIZ7,
        SiZ7,
        BRZ7
    }
    public enum Security
    {
        SBER,
        SBERP,
        GAZP,
        LKOH,
        MTSS,
        MGNT,
        MOEX,
        NVTK,
        NLMK,
        RASP,
        VTBR,
        RTKM,
        ROSN,
        AFLT,
        AKRN,
        AFKS,
        PHOR,
        GMKN,
        CHMF,
        SNGS,
        URKA,
        FEES,
        ALRS,
        APTK,
        YNDX,
        MTLRP,
        MAGN,
        BSPB,
        MTLR
    }
}