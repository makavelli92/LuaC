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

namespace ConsoleApp2
{
    public enum Class 
    {
        SPBFUT = 1,
        TQBR = 2
    }

    class Program
    {
        private static Mutex mutexCmd;
        private static Mutex mutexDat;

        private const string mutexCommand = "MutexForCommand";
        private const string mutexData = "MutexForData";

        static void Main(string[] args)
        {
            //  int temp = (int)Class.SPBFUT;
            //List<double> temp = new List<double> { 1, 1, 1, 1, 1, 1 };
            //temp.RemoveAll(x => x == 1);

            //foreach (double i in temp)
            //{
            //    Console.WriteLine(i);
            //}
            //Console.WriteLine();
            //Console.ReadLine();



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

            SW_Flag.BaseStream.Seek(0, SeekOrigin.Begin);
            SW_Flag.Write("\0");
            SW_Flag.Flush();

            SW_FlagCommand.BaseStream.Seek(0, SeekOrigin.Begin);
            SW_FlagCommand.Write("\0");
            SW_FlagCommand.Flush();
            SW_Memory.BaseStream.Seek(0, SeekOrigin.Begin);
            // Очищает память, заполняя "нулевыми байтами"
            for (int i = 0; i < 200000; i++)
            {
                SW_Memory.Write("\0");
            }
            SW_Memory.Flush();
            string Data = "";
            for (int i = Data.Length; i < 128; i++) Data += "\0";
            SW_Command.BaseStream.Seek(0, SeekOrigin.Begin);
            //Записывает строку
            SW_Command.Write(Data);
            //Сохраняет изменения в памяти
            SW_Command.Flush();

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
                Console.WriteLine("OPEN");
            }
            catch
            {
                mutexDat = new Mutex(false, mutexData);
                Console.WriteLine("CREATE");
            }
            
            
                //mutexDat.ReleaseMutex();
            
            
          //  mutexCmd.ReleaseMutex();
            mutexDat.Close();
            mutexCmd.Close();
            mutexDat.Dispose();
        }
    }
}
