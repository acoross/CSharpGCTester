using System.IO;
            
namespace GCTester
{
    class Vector
    {
        public double X;
        public double Y;

        public Vector(){}

        public Vector(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static double SqDist(Vector v1, Vector v2)
        {
            var diffX = (v1.X - v2.X);
            var diffY = (v1.Y - v2.Y);

            return diffX * diffX + diffY * diffY;
        }
    }

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

        public void Start()
        {
            if (this is PC)
            {
                PC.workingPcCount++;
            }
            else if (this is NPC)
            {
                NPC.workingNpcCount++;
            }

            int delay = game.rand.Next(100, 1200);
            OnTick(delay);
        }

        public void Stop()
        {
            run = false;
        }

        // can move 4 times per second, 
        // but only 50% of characters are actively moving
        // so, average action rate is 2 times per second.
        // -> 
        void OnTick(int delay)
        {
            game.Post(() =>
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

                if (run)
                {
                    int delay2 = game.rand.Next(400, 600);
                    OnTick(delay2);
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
            }, delay);
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

                game.BroadcastToUsers(ms.ToArray());
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

                game.BroadcastToUsers(ms.ToArray());
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
