using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GCTester
{
    class MainClass
    {
        static async Task MainAsync()
        {
            try
            {
                Console.WriteLine("Hello World!");

                int SERIALIZER_COUNT = 4;
                int GAME_COUNT_PER_SERIALIZER = 1;
                int USER_COUNT_PER_GAME = 100;

                List<JobSerializer> serializers = new List<JobSerializer>();

                List<PerfMon> perfMons = new List<PerfMon>();
                List<Game> games = new List<Game>();

                for (int i = 0; i < SERIALIZER_COUNT; ++i)
                {
                    var serializer = new JobSerializer();
                    serializers.Add(serializer);

                    var perfMon = new PerfMon($"s{i}", serializer);
                    perfMon.Start();
                    perfMons.Add(perfMon);

                    for (int j = 0; j < GAME_COUNT_PER_SERIALIZER; ++j)
                    {
                        var num = i;
                        Game game = new Game(serializer, num, USER_COUNT_PER_GAME);
                        game.Init();
                        games.Add(game);
                    }

                    serializer.Start();
                }

                bool perfLogRun = true;
                var taskPerfLog = Task.Run(() =>
                {
                    DateTime serverUpTime = DateTime.Now;

                    while (perfLogRun)
                    {
                        var current = DateTime.Now;
                        var diff = current - serverUpTime;

                        Console.WriteLine("=======================================================");
                        Console.WriteLine($"up:{serverUpTime}, current:{current}, diff:{diff}");
                        Console.WriteLine($"pc: {PC.workingPcCount}, npc: {NPC.workingNpcCount}");
                        Console.WriteLine($"buffer: {NetBuffer.AllocStateString()}");
                        Console.WriteLine("=======================================================");
                        foreach (var pm in perfMons)
                        {
                            Console.WriteLine(pm.PerfString());
                        }
                        Console.WriteLine("=======================================================");
                        Console.WriteLine();

                        if (perfLogRun)
                        {
                            System.Threading.Thread.Sleep(3000);
                        }
                    }
                });

                for (;;)
                {
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'q')
                    {
                        foreach (var s in serializers)
                        {
                            s.Stop();
                        }
                        break;
                    }
                }

                Console.WriteLine("wait for jobSerializer end...");

                foreach (var s in serializers)
                {
                    await s.Completion;
                }

                perfLogRun = false;
                await taskPerfLog;
            }
            finally
            {
                Console.WriteLine("bye!");
            }
        }

        public static void Main(string[] args)
        {
            MainAsync().Wait();
        }
    }
}
