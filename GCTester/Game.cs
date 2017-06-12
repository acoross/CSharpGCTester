using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GCTester
{
    class PC : Character
    {
        public static int workingPcCount = 0;

        byte[] sendQueue = new byte[1024];

        public PC(Game game, int name)
            : base(game, name)
        {
        }

        public void Send(byte[] data)
        {
            Buffer.BlockCopy(data, 0, sendQueue, 0, data.Length);
        }

        protected override Character FindNearestEnemy()
        {
            return game.FindNearestNPC(pos);
        }

        protected override void OnDie()
        {
            pos = game.RandomPos();
            hp = 10;
        }
    }

    class NPC : Character
    {
        public static int workingNpcCount = 0;

        public NPC(Game game, int name)
            : base(game, name)
        {
        }

        protected override Character FindNearestEnemy()
        {
            return game.FindNearestPC(pos);
        }

        protected override void OnDie()
        {
            game.RemoveNPC(this);

            game.Post(() =>
            {
                game.AddNewNpc(this.name);
            });
        }
    }

    class Game
    {
        readonly int USER_COUNT;
        int NPC_COUNT { get { return USER_COUNT * 10; } }

        public Random rand { get; } = new Random(DateTime.Now.Millisecond);
        ConcurrentDictionary<int, PC> pcs = new ConcurrentDictionary<int, PC>();
        ConcurrentDictionary<int, NPC> npcs = new ConcurrentDictionary<int, NPC>();
        BufferBlock<Action> jobQueue = new BufferBlock<Action>();

        public readonly int number;

        public Game(int num, int userCount)
        {
            this.USER_COUNT = userCount;
            this.number = num;
        }

        public async Task Run()
        {
            Initialize();

            // run
            for (;;)
            {
                var job = await jobQueue.ReceiveAsync();
                job();
            }
        }

        void Initialize()
        {
            for (int i = 0; i < USER_COUNT; ++i)
            {
                var pc = new PC(this, i);
                pc.pos = RandomPos();
                pcs.TryAdd(i, pc);
                pc.Start();
            }

            for (int i = 0; i < NPC_COUNT; ++i)
            {
                AddNewNpc(i);
            }
        }

        public void AddNewNpc(int name)
        {
            var npc = new NPC(this, name);
            npc.pos = RandomPos();
            npcs.TryAdd(name, npc);
            npc.Start();
        }

        public void Post(Action job, int delay = 0)
        {
            if (delay > 0)
            {
                Task.Delay(delay).ContinueWith((t) =>
                {
                    jobQueue.SendAsync(job);
                });
            }
            else
            {
                jobQueue.SendAsync(job);
            }
        }

        public void BroadcastToUsers(byte[] data)
        {
            foreach (var pc in pcs.Values)
            {
                pc.Send(data);
            }
        }

        public Vector RandomPos()
        {
            return new Vector(rand.NextDouble() * 100 - 50, rand.NextDouble() * 100 - 50);
        }

        public PC FindNearestPC(Vector pos)
        {
            double minSqDist = double.MaxValue;
            PC nearest = null;

            foreach (var pc in pcs.Values)
            {
                var sqDist = Vector.SqDist(pc.pos, pos);
                if (minSqDist > sqDist)
                {
                    minSqDist = sqDist;
                    nearest = pc;
                }
            }

            return nearest;
        }

        public NPC FindNearestNPC(Vector pos)
        {
            double minSqDist = double.MaxValue;
            NPC nearest = null;

            foreach (var npc in npcs.Values)
            {
                var sqDist = Vector.SqDist(npc.pos, pos);
                if (minSqDist > sqDist)
                {
                    minSqDist = sqDist;
                    nearest = npc;
                }
            }

            return nearest;
        }

        public void RemoveNPC(NPC npc)
        {
            NPC removed;
            if (npcs.TryRemove(npc.name, out removed))
            {
                removed.Stop();
            }
        }
    }
}
