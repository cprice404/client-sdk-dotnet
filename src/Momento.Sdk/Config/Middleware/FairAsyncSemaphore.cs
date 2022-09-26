using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Momento.Sdk.Config.Middleware
{
    public class FairAsyncSemaphore
    {
        private readonly Channel<bool> _ticketChannel;

        public FairAsyncSemaphore(int numTickets)
        {
            //Console.WriteLine($"Creating semaphore with {numTickets} tickets");
            _ticketChannel = Channel.CreateBounded<bool>(numTickets);

            for (var i = 0; i < numTickets; i++)
            {
                bool success = _ticketChannel.Writer.TryWrite(true);
                if (!success)
                {
                    throw new ApplicationException("Unable to initialize async channel");
                }
            }
        }

        public async Task WaitOne()
        {
            //Console.WriteLine($"Waiting for semaphore");
            await _ticketChannel.Reader.ReadAsync();
        }

        public void Release()
        {
            var balanced = _ticketChannel.Writer.TryWrite(true);
            if (!balanced)
            {
                throw new ApplicationException("more releases than waits! These must be 1:1")
            }
        }
    }
}

