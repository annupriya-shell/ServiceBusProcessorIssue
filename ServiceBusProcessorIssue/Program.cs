using Azure.Messaging.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServicebusSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Program p = new Program();
            Console.WriteLine("Hello World!");

            await p.CreateConsumer();

            await Task.Delay(Timeout.Infinite);

        }




        public async Task CreateConsumer()
        {


            var client = new ServiceBusClient("Service bus connnection string");
            //lock duration configured to 5 min for the queue
            var proc = client.CreateProcessor("managedQueue", options: new ServiceBusProcessorOptions
            {

                AutoCompleteMessages = false,
                PrefetchCount = 1,
                MaxAutoLockRenewalDuration = TimeSpan.FromSeconds(180)
            });
            proc.ProcessMessageAsync += MessageHandler;
            proc.ProcessErrorAsync += ErrorHandler;
            Console.WriteLine("Max auto lock renew :" + proc.MaxAutoLockRenewalDuration);
            // start processing 
            await proc.StartProcessingAsync();
        }


        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {

                string body = args.Message.Body.ToString();
                Console.WriteLine($"Received: {body}");
                await Task.Delay(120000);//2 min long running job message should auto renew after

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception In Handler:" + ex);
            }

        }

        // handle any errors when receiving messages
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());

            return Task.CompletedTask;
        }
    }


}