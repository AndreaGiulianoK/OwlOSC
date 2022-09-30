using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OwlOSC
{

    public class UDPListener : IDisposable
    {

        ~UDPListener()
        {
            Dispose();
        }

        private const int _MAX_QUEUE_SIZE = 1000;

        public int Port { get; private set; }

        UdpClient receivingUdpClient;
        IPEndPoint RemoteIpEndPoint;

        bool queueMessages;

        ConcurrentQueue<OscPacket> packetQueue;

        public int messagesQueued { get { return packetQueue.Count; } }

        CancellationTokenSource cancelTokenSource;
        CancellationToken token;

        List<AddressHandler> addressCallbacks;

        /// <summary>
        /// Create a new OSC Listener and start threaded receive loop.
        /// </summary>
        /// <param name="port">Listening port</param>
        private UDPListener(int port)
        {
            Port = port;
            packetQueue = new ConcurrentQueue<OscPacket>();
            addressCallbacks = new List<AddressHandler>();

            System.Text.RegularExpressions.Regex.CacheSize = 32;

            // try to open the port 10 times, else fail
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    receivingUdpClient = new UdpClient(port);
                    break;
                }
                catch (Exception)
                {
                    // Failed in ten tries, throw the exception and give up
                    if (i >= 9)
                        throw;

                    Thread.Sleep(5);
                }
            }
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            //
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            Task.Run(() => BeginListeningAsync(token));
        }

        /// <summary>
		/// Create listener and start a threaded read loop.
        /// If 'queueMessages' is TRUE, messages are queued, if FALSE or none, the callbacks will be immediate e no message will be quequed.
        /// If 'queueMessages' is TRUE the messages must be dequeued manually with the 'ReadQueuedMessage' or 'ReadAllQueuedMessages' methods.
		/// NOTE: Unity3D require 'queueMessages' is TRUE.
        /// </summary>
		/// <param name="port">>Listening port</param>
		/// <param name="queueMessages">Enqueue the received messages and don't evaluate addresses callbacks</param>
		/// <returns>Listener instance</returns>
        public UDPListener(int port, bool queueMessages = true) : this(port)
        {
            this.queueMessages = queueMessages;
        }

        private async Task BeginListeningAsync(CancellationToken token)
        {
            while (true || !token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                if (closing) throw new Exception("UDPListener has been closed.");
                try
                {
                    var result = await receivingUdpClient.ReceiveAsync();
                    ProcessData(result.Buffer);
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if disposed. This happens when closing the listener
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
            Console.WriteLine("End Listenting Async");
        }

        private void StartDequeueLoop()
        {
            Task.Run(() => DequeueLoop(token));
        }

        private async Task DequeueLoop(CancellationToken token)
        {
            while (true || !token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
                if (closing) throw new Exception("UDPListener has been closed.");

                if (packetQueue.Count() > 0)
                {
                    OscPacket packet;
                    packetQueue.TryDequeue(out packet);
                    if (packet != null)
                        try
                        {
                            EvaluateAddresses(packet);
                        }
                        catch { }
                }
                //VERY IMPORTANT TO AVOID UDP PACKET LOSS !!!!!!
                await Task.Delay(1);
            }
        }

        private void ProcessData(Byte[] bytes)
        {
            OscPacket packet = null;
            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    packet = OscPacket.GetPacket(bytes);
                }
                catch (Exception e)
                {
                    // If there is an error reading the packet, null is sent to the callback
                    Console.WriteLine("Error reading OSC bytes: " + e.Message);
                }

                if (packet != null)
                {
                    Console.WriteLine($"Received {bytes.Length} byte of data");
                    //sycronous callback
                    if (!queueMessages)
                    {
                        try
                        {
                            EvaluateAddresses(packet);
                        }
                        catch { }
                    }
                    else
                    {  //enqueue message
                        //if reached max queue size discard exceding message
                        while (packetQueue.Count >= _MAX_QUEUE_SIZE)
                            packetQueue.TryDequeue(out OscPacket tmp);
                        packetQueue.Enqueue(packet);
                    }
                }
            }
        }

        /// <summary>
        /// Evalauate address pattern and Invoke callback (Thread safe)
        /// </summary>
        /// <param name="packet"></param>
        private void EvaluateAddresses(OscPacket packet)
        {
            if (!packet.IsBundle)
            {
                var address = ((OscMessage)packet).Address;
                if (Utils.ValideteAddress(address))
                {
                    addressCallbacks.Where(x => Utils.MatchAddress(x.address, address)).ToList().ForEach(x => x.callback.Invoke(packet));
                }
                else
                {
                    Console.WriteLine("Received message address malformed");
                }
            }
            else
            {
                var bundle = (OscBundle)packet;
                bundle.Messages.ForEach(m =>
                {
                    if (Utils.ValideteAddress(m.Address))
                    {
                        addressCallbacks.Where(x => Utils.MatchAddress(x.address, m.Address)).ToList().ForEach(x => x.callback.Invoke(m));
                    }
                    else
                    {
                        Console.WriteLine("Received message address malformed");
                    }
                });
            }
        }

        /// <summary>
        /// Add an handler for specific address.
        /// </summary>
        /// <param name="address">addres of message</param>
        /// <param name="handleOscPacket">Packet handler</param>
        /// <returns>True if address is valid and added, FALSE if not</returns>
        public bool AddAddress(string address, HandleOscPacket handleOscPacket)
        {
            AddressHandler addressHandler = new AddressHandler(address, handleOscPacket);
            if (addressHandler.isValid)
            {
                addressCallbacks.Add(addressHandler);
                Console.WriteLine($"Address Added: {address}");
            }
            else
            {
                Console.WriteLine($"Address not valid : {address}");
            }
            return addressHandler.isValid;
        }

        bool closing = false;

        public void Dispose()
        {
            closing = true;
            cancelTokenSource.Cancel();
            receivingUdpClient.Close();
            receivingUdpClient.Dispose();
        }

        /// <summary>
        /// Get single message from queue. (Thread Safe).
        /// If 'evaluateAddressCallback' is TRUE matches address patterns and invoke callbacks.
        /// NOTE: if 'queueMessages' of this UDPListener is FALSE no message will be queued and returned from this method.
        /// WARNING! The mesasge is removed from evaluation loop so if 'evaluateAddressCallback' is FALSE no callback will occour for returned message.
        /// WARNING! Can be NULL, require a nullcheck.
        /// </summary>
        /// <param name="evaluateAddressCallback">Evaluate and Call address callback</param>
        /// <returns>message/bundle packet, can be NULL</returns>      
        public OscPacket ReadQueuedMessage(bool evaluateAddressCallback = true)
        {
            if (closing) throw new Exception("UDPListener has been closed.");

            if (packetQueue.Count() > 0)
            {
                OscPacket packet = null;
                packetQueue.TryDequeue(out packet);
                if (evaluateAddressCallback && packet != null)
                {
                    try
                    {
                        EvaluateAddresses(packet);
                    }
                    catch { }
                }
                return packet;
            }
            else
                return null;
        }

        /// <summary>
        /// Read all messages in queue. (Thread Safe).
        /// If 'evaluateAddressCallback' is TRUE matches address patterns and invoke callbacks.
        /// NOTE: if 'queueMessages' of this UDPListener is FALSE no message will be queued.
        /// </summary>
        /// <param name="evaluateAddressCallback">Evaluate and Call address callback</param>
        public void ReadAllQueuedMessages(bool evaluateAddressCallback = true){
            while(messagesQueued > 0){
                ReadQueuedMessage(evaluateAddressCallback);
            }
        }

    }
}
