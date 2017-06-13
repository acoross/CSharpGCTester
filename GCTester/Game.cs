using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GCTester
{
    class Game
    {
        readonly int USER_COUNT;
        int NPC_COUNT { get { return USER_COUNT * 10; } }

        public Random rand { get; } = new Random(DateTime.Now.Millisecond);
        ConcurrentDictionary<int, PC> pcs = new ConcurrentDictionary<int, PC>();
        ConcurrentDictionary<int, NPC> npcs = new ConcurrentDictionary<int, NPC>();

        public JobSerializer serializer { get; private set; }

        public readonly int number;

        public Game(JobSerializer serializer, int num, int userCount)
        {
            this.serializer = serializer;
            this.USER_COUNT = userCount;
            this.number = num;
        }

        public void Init()
        {
            Initialize();
        }

        void Initialize()
        {
            Post(() =>
            {
                for (int i = 0; i < USER_COUNT; ++i)
                {
                    AddNewPc(i);
                }

                for (int i = 0; i < NPC_COUNT; ++i)
                {
                    AddNewNpc(i);
                }
            });
        }

        public void AddNewPc(int name)
        {
            var pc = new PC(this, name);
            pc.pos = RandomPos();
            pcs.TryAdd(name, pc);
            pc.Start();
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
            serializer.Post(job, delay);
        }

        public void BroadcastToUsers(NetBuffer buffer)
        {
            foreach (var pc in pcs.Values)
            {
                pc.Send(buffer);
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
