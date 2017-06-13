using System;

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

        public void Send(NetBuffer buffer)
        {
            Buffer.BlockCopy(buffer.data, 0, sendQueue, 0, buffer.data.Length);
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
}
