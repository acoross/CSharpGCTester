using System.IO;
using System.Threading.Tasks;

namespace GCTester
{
    abstract class Character
    {
        public readonly int name;
        public readonly Game game;
        public Vector pos = new Vector { X = 0, Y = 0 };

        public int hp = 10;

        bool run = true;

        public Character(Game game, int name)
        {
            this.game = game;
            this.name = name;
        }

        public async void Start()
        {
            if (this is PC)
            {
                PC.workingPcCount++;
            }
            else if (this is NPC)
            {
                NPC.workingNpcCount++;
            }

            OnTick();

            //int delay = game.rand.Next(100, 1200);

            //await Task.Delay(delay);

            //try
            //{
            //    while (run)
            //    {
            //        int cmd = game.rand.Next(2);
            //        if (cmd == 0)
            //        {
            //            Move();
            //        }
            //        else
            //        {
            //            AttackNerest();
            //        }

            //        if (run)
            //        {
            //            await Task.Delay(game.rand.Next(400, 600));
            //        }
            //    }
            //}
            //finally
            //{
            //    if (this is PC)
            //    {
            //        PC.workingPcCount--;
            //    }
            //    else if (this is NPC)
            //    {
            //        NPC.workingNpcCount--;
            //    }
            //}
        }

        void OnTick()
        {
            game.Post(() =>
            {
                if(run)
                {
                    int cmd = game.rand.Next(2);
                    if (cmd == 0)
                    {
                        Move();
                    }
                    else
                    {
                        AttackNerest();
                    }

                    OnTick();
                }
                else
                {
                    if (this is PC)
                    {
                        PC.workingPcCount--;
                    }
                    else if (this is NPC)
                    {
                        NPC.workingNpcCount--;
                    }
                }
            }, game.rand.Next(400, 600));
        }

        public void Stop()
        {
            run = false;
        }

        void Move()
        {
            double x = game.rand.NextDouble() * 2 - 1;
            double y = game.rand.NextDouble() * 2 - 1;

            MoveTo(x, y);
        }

        void MoveTo(double x, double y)
        {
            pos.X += x;
            pos.Y += y;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(name);
                bw.Write(pos.X);
                bw.Write(pos.Y);
                bw.Write(x);
                bw.Write(y);

                var buffer = new NetBuffer(ms.ToArray());
                game.BroadcastToUsers(buffer);
            }
        }

        int getDir(double num)
        {
            if (num >= 1)
                return 1;
            if (num <= -1)
                return -1;
            else
                return 0;
        }

        void AttackNerest()
        {
            var target = FindNearestEnemy();
            if (target == null)
            {
                Move();
                return;
            }

            var distSq = Vector.SqDist(pos, target.pos);
            if (distSq > 25)
            {
                int x = getDir(target.pos.X - pos.X);
                var y = getDir(target.pos.Y - pos.Y);
                MoveTo(x, y);
                return;
            }

            target.Damaged();

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(name);
                bw.Write(target.name);
                bw.Write("damage");
                bw.Write(1);

                var buffer = new NetBuffer(ms.ToArray());
                game.BroadcastToUsers(buffer);
            }
        }

        protected abstract Character FindNearestEnemy();

        void Damaged()
        {
            hp--;
            if (hp <= 0)
            {
                OnDie();
            }
        }

        protected abstract void OnDie();
    }
}
