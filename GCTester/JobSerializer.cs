using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GCTester
{
    class JobSerializer
    {
        int run = 0;
        BufferBlock<Action> jobQueue = new BufferBlock<Action>();
        TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

        public int requestTotal = 0;
        public int responseTotal = 0;
        public int active
        {
            get
            {
                var res = responseTotal;
                var req = requestTotal;
                return req - res;
            }
        }

        public async void Post(Action job, int delay = 0)
        {
            if (delay > 0)
            {
                await Task.Delay(delay);
            }

            requestTotal++;
            jobQueue.Post(job);
        }

        public async void Start()
        {
            run = 1;

            try
            {
                while (run > 0)
                {
                    await Task.Yield();

                    IList<Action> jobs;
                    if (jobQueue.TryReceiveAll(out jobs))
                    {
                        foreach (var job in jobs)
                        {
                            job();
                            responseTotal++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            finally
            {
                Console.WriteLine("job done");
                tcs.SetResult(0);
            }
        }

        public void Stop()
        {
            if (1 == Interlocked.CompareExchange(ref run, 0, 1))
            {
                jobQueue.Complete();
            }
        }

        public Task Completion
        {
            get
            {
                return tcs.Task;
            }
        }

        public string ProcessingState()
        {
            return $"req/res/act:{requestTotal}/{requestTotal}/{active}";
        }
    }
}
