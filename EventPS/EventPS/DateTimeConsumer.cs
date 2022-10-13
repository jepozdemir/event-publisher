using System;
using System.Threading.Tasks;

namespace EventPS
{
    public class DateTimeConsumer : IConsumer<DateTime>
    {
        public Task HandleEventAsync(DateTime eventMessage)
        {
            DateTime = eventMessage;
            Console.WriteLine($"new datetime event occured : {eventMessage}");
            return Task.CompletedTask;
        }

        // For testing
        public static DateTime DateTime { get; set; }
    }
}
