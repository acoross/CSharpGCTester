using System;

namespace GCTester
{
    class PerfMon
    {
        readonly string name;
        readonly JobSerializer serializer;

        DateTime last;
        int count = 0;
        double avr = 0;

        bool first = true;

        public PerfMon(string name, JobSerializer serializer)
        {
            this.name = name;
            this.serializer = serializer;
            last = DateTime.Now;
        }

        public void Start()
        {
            loop();
        }

        public string PerfString()
        {
            return $"[{name}][{count}] avr: {avr:n2}, \t{serializer.ProcessingState()}";
        }

        void loop()
        {
            serializer.Post(() =>
            {
                var current = DateTime.Now;
                var diff = current - last;
                if (diff > TimeSpan.FromMilliseconds(66))
                {
                    Console.WriteLine($"[{name}][{count}] io pending: {diff.Milliseconds}");
                }

                if (first)
                {
                    first = false;
                    avr = diff.TotalMilliseconds;
                }
                else
                {
                    avr *= 0.995;
                    avr += diff.TotalMilliseconds * 0.005;
                }

                count++;
                last = current;
                loop();
            }, 1);
        }
    }
}
