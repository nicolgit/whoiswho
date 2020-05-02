using System;

namespace fake_data_loader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var loader = new Loader();

            loader.Run().Wait();
        }
    }
}

