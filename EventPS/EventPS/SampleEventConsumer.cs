using System;
using System.Threading.Tasks;

namespace EventPS
{
    public class SampleEventConsumer : IConsumer<SampleEvent>
    {
        public Task HandleEventAsync(SampleEvent eventMessage)
        {
            Console.WriteLine($"new sample event occured : {eventMessage.Arg1}");
            return Task.CompletedTask;
        }
    }

    public class SampleEvent
    {
        public string Arg1 { get; set; }
    }
}
