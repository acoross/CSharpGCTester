using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GCTester
{
    class MainClass
    {
        static void PerfMon(Game game, DateTime last, int count, double avr)
        {
            var current = DateTime.Now;
            var diff = current - last;
            if (diff > TimeSpan.FromMilliseconds(66))
            {
                Console.WriteLine($"[{game.number}][{count}] io pending: {diff.Milliseconds}");
            }

            if (double.Epsilon > Math.Abs(avr))
            {
                avr = diff.TotalMilliseconds;
            }
            else
            {
                avr *= 0.995;
                avr += diff.TotalMilliseconds * 0.005;
            }

            if (count % 100 == 0)
            {
                if (game.number == 0)
                {
                    Console.WriteLine($"pc: {PC.workingPcCount}, npc: {NPC.workingNpcCount}");    
                }
                Console.WriteLine($"[{game.number}][{count}] avr: {avr}");
            }

            last = current;

            game.Post(() =>
            {
                PerfMon(game, last, count + 1, avr);
            }, 16);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            int gameCount = 4;
            int userCount = 100;

            Task[] tasks = new Task[gameCount];
            for (int i = 0; i < gameCount; ++i)
            {
                var num = i;
                tasks[i] = Task.Run(() =>
                {
                   Game game = new Game(num, userCount);

                   var last = DateTime.Now;
                   game.Post(() =>
                   {
                       PerfMon(game, last, 0, 0);
                   }, 33);

                   game.Run().Wait();
               });
            }
            Task.WaitAll(tasks);
        }
    }
}
