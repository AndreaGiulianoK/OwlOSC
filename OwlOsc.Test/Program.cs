using System;
using System.Collections;
using System.Collections.Generic;
using OwlOSC;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace OwlOsc.Test
{
    class Program
    {
        static int localPort = 1234;
        static int remotePort = 1234;

        static bool ShowHelpRequired(IEnumerable<string> args){
            return args.Select(s => s.ToLowerInvariant())
                .Intersect(new[] {"help", "/?", "--help", "-h"}).Any();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("OwlOSC TEST");
            Console.WriteLine("USE: [OPTIONS] [filePath]");
            Console.WriteLine("Options : [-send] [-receive] [-receiveloop] [-sendTicks] [-receiveTicks] [-sendFile] [-receiveFile]");
            Console.WriteLine("Help: [help] -h --help /?");

            if(ShowHelpRequired(args)){
                Console.WriteLine("HELP\n");
                Console.WriteLine("Options");
                Console.WriteLine("send             send debug message and bundle");
                Console.WriteLine("receive          receive single message and bundle");
                Console.WriteLine("receiveloop      receive loop async (don't close)");
                Console.WriteLine("sendTicks        Speed test: send message with time ticks");
                Console.WriteLine("receiveTicks     Speed test: receive ticks mesage and evaluate delay");
                Console.WriteLine("sendFile         send single file, require [filePath] option");
                Console.WriteLine("receiveFile      receive single file, require [filePath] option");
                Console.WriteLine("\nUSAGE:");
                Console.WriteLine("     OwlOsc.Test -send");
                Console.WriteLine("     OwlOsc.Test -receiveloop");
                Console.WriteLine("     OwlOsc.Test -sendFile path2file2read");
                Console.WriteLine("     OwlOsc.Test -receiveFile path2file2write");
            }

            if(args.Length < 1 || args.Length > 2)
                return;
            //
            if(args[0] == "send"){
                OscMessage message;
                var sender = new UDPSender("127.0.0.1", remotePort);
                message = new OscMessage("/test/1", 23, 42.01f, "hello world");
                sender.Send(message);
                Console.WriteLine($"Mesage Sent: " + message.ToString());
                var bundle = new OscBundle(new Timetag(DateTime.UtcNow).Tag, new OscMessage("/test",1.34f),new OscMessage("/test/subtest",2.3434d), new OscMessage("/c",3));
                sender.Send(bundle);
                Console.WriteLine("Bundle Sent: " + bundle.ToString());
                sender.Close();
            }
            if(args[0] == "receive"){
                var listener = new UDPListener(localPort);
                OscPacket message=null;
                while(message == null){
                    message = listener.Receive();
                    Task.Delay(1);
                }
                listener.Close();
                GetMessage(message);
            }
            if(args[0] == "receiveloop"){
                
                var listener = new UDPListener(localPort, GetMessage);

                Console.WriteLine("\nPress any key to stop and exit...");
                Console.ReadKey();
                listener.Close();
            }
            //
            if(args[0] == "sendTicks"){
                OscMessage message;
                var sender = new UDPSender("127.0.0.1", remotePort);
                for(int i = 0; i< 10001; i++){
                    double tick = System.DateTime.Now.Ticks;
                    message = new OscMessage("/test", tick);
                    sender.Send(message);
                    Console.WriteLine($"Sent: {tick}");
                    //Task.Delay(1);
                    //System.Threading.Thread.Sleep(1);
                }
                sender.Close();
            }
            if(args[0] == "receiveTicks"){
                var listener = new UDPListener(localPort, ReceiveTicks);
                Console.WriteLine("\nPress any key to stop and exit...");
                Console.ReadKey();
                listener.Close();
            }

            //
            if(args[0] == "sendFile"){
                Console.WriteLine("sendFile");
                if(args.Length != 2)
                    throw new Exception("Invalid parameters");
                if(File.Exists(args[1])){
                    var data = File.ReadAllBytes(args[1]);
                    var message = new OscMessage("/file", data);
                    var sender = new UDPSender("127.0.0.1",remotePort);
                    try{
                        sender.Send(message);
                        Console.WriteLine($"File {args[1]} Sent");
                    }catch (Exception e){
                        Console.WriteLine("Error Sending file: " + e.Message);
                    }
                    sender.Close();
                }else{
                    throw new Exception("No file found!");
                }
            }
            if(args[0] == "receiveFile"){
                Console.WriteLine("receiveFile");
                if(args.Length != 2)
                    throw new Exception("Invalid parameters");
                var listner = new UDPListener(localPort);
                OscPacket message=null;
                while(message == null){
                    message = listner.Receive();
                    Task.Delay(1);
                }
                listner.Close();
                if(message != null){
                    byte[] data = (byte[])((OscMessage)message).Arguments[0];
                    if(data != null && data.Length > 0){
                        File.WriteAllBytes(args[1],data);
                        Console.WriteLine($"File {args[1]} Received");
                    }else{
                        throw new Exception("No DATA!");
                    }
                }else{
                    throw new Exception("Malformed OSC message");
                }
            }
        }
        
        static void GetMessage (OscPacket packet){
            if(packet == null){
                Console.WriteLine("Malformed OSC Packet");
                return;
            }
            if(packet.IsBundle){
                var bundleReceived = (OscBundle)packet;
                Console.WriteLine(bundleReceived.ToString());
            }else{
                var messageReceived = (OscMessage)packet;
                Console.WriteLine(messageReceived.ToString());
            }
        }
        
        static double lastTick;
        static double delta;
        
        static int ticks;
        static double sum;
        static double average;

        static void ReceiveTicks (OscPacket packet){
            try{
                var messageReceived = (OscMessage)packet;
                double tick = (double)messageReceived.Arguments[0];
                if(ticks == 0){
                    lastTick = tick;
                }else{
                    delta = (tick - lastTick)/System.TimeSpan.TicksPerMillisecond;
                    lastTick = tick;
                    sum += delta;
                    Console.WriteLine($"Message Received: {ticks} : '{messageReceived.Address}' -> {tick} @ Delta {delta}");
                }
                ticks++;
            }catch (Exception e){
                throw new Exception("Error on elaborate ticks message: " + e.Message);
            }
            if(ticks % 100 == 0 && ticks > 0){
                average = sum / ticks;
                Console.WriteLine($"\n\n Partial Delta Average Speed: {average} ms @ {ticks}\n\n");
            }
        }
    }
}
