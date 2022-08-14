using System;
using OwlOSC;
using System.Threading.Tasks;
using System.Linq;

namespace OwlOsc.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("OwlOSC TEST");
            Console.WriteLine("Params: [send][receive][receiveloop][sendTicks][receiveTicks]");
            if(args.Length != 1)
                return;
            //
            if(args[0] == "send"){
                OscMessage message;
                var sender = new UDPSender("127.0.0.1", 1234);
                message = new OscMessage("/test/1", 23, 42.01f, "hello world");
                sender.Send(message);
            }
            if(args[0] == "receive"){
                var listener = new UDPListener(1234);
                OscMessage messageReceived = null;
                while (messageReceived == null)
                {
                    messageReceived = (OscMessage)listener.Receive();
                    Task.Delay(1);
                }
                string values = "";
                messageReceived.Arguments.ForEach(x=>{
                    values += $"{x.ToString()}; ";
                });
                Console.WriteLine($"Message Received: '{messageReceived.Address}' -> {values}");
                listener.Close();
            }
            if(args[0] == "receiveloop"){
                
                var listener = new UDPListener(1234, Callback);

                Console.WriteLine("\nPress any key to stop and exit...");
                Console.ReadKey();
                listener.Close();
            }
            //
            if(args[0] == "sendTicks"){
                OscMessage message;
                var sender = new UDPSender("127.0.0.1", 1234);
                for(int i = 0; i< 1000; i++){
                    double tick = System.DateTime.Now.Ticks;
                    message = new OscMessage("/test", tick);
                    sender.Send(message);
                    Console.WriteLine($"Sent: {tick}");
                    Task.Delay(1);
                }
            }
            if(args[0] == "receiveTicks"){
                
                var listener = new UDPListener(1234, ReceiveTicks);

                Console.WriteLine("\nPress any key to stop and exit...");
                Console.ReadKey();
                listener.Close();
            }
        }

        static void Callback (OscPacket packet){
            try{
                var bundle = (OscBundle)packet;
                int b=0;
                bundle.Messages.ForEach(m=>{
                    b++;
                    string values = "";
                    m.Arguments.ForEach(x=>{
                        values += $"{x.ToString()}; ";
                    });
                    Console.WriteLine($"Message Received: {b}: '{m.Address}' -> {values}");
                });
            }catch{}
            try{
                var messageReceived = (OscMessage)packet;
                string values = "";
                messageReceived.Arguments.ForEach(x=>{
                    values += $"{x.ToString()}; ";
                });
                Console.WriteLine($"Message Received: '{messageReceived.Address}' -> {values}");
            }catch{}

        }
        
        static double lastTick;
        static float delta;

        static void ReceiveTicks (OscPacket packet){
            try{
                var messageReceived = (OscMessage)packet;
                double tick = (double)messageReceived.Arguments[0];
                delta = (float)((tick - lastTick)/System.TimeSpan.TicksPerMillisecond);
                lastTick = tick;
                Console.WriteLine($"Message Received: '{messageReceived.Address}' -> {tick} @ Delta {delta}");
            }catch{}
        }

    }
}
