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

        ~UDPListener(){
            Dispose();
        }

		private const int _MAX_QUEUE_SIZE = 10000;

        public int Port { get; private set; }

        UdpClient receivingUdpClient;
        IPEndPoint RemoteIpEndPoint;

        HandleOscPacket OscPacketCallback = null;

        bool delayedAddressCallback;

		ConcurrentQueue<OscPacket> packetQueue;

        CancellationTokenSource cancelTokenSource;
        CancellationToken token;

        List<AddressHandler> addressCallbacks;

		/// <summary>
		/// Start OSC Listener.
		/// </summary>
		/// <param name="port">Listening port</param>
        public UDPListener(int port)
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
		/// Start Listner with immediate Packet Callback.
		/// </summary>
		/// <param name="port">Listening port</param>
		/// <param name="callback">Packet received callback, Packet can be NULL</param>
		/// <returns>Listner instance</returns>
        public UDPListener(int port, HandleOscPacket callback) : this(port)
        {
            this.OscPacketCallback = callback;
        }

        /// <summary>
		/// Start Listner with immediate Address match Callback.
        /// If FALSE, a threaded loop will dequeue every millisecond
		/// </summary>
		/// <param name="port">>Listening port</param>
		/// <param name="delayedAddressCallback">Delayed callback</param>
		/// <returns>Listner instance</returns>
        public UDPListener(int port, bool delayedAddressCallback = false) : this(port)
        {
            this.delayedAddressCallback = delayedAddressCallback;
            if(delayedAddressCallback)
                StartDequeueLoop();
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

		private void StartDequeueLoop(){
			Task.Run(() => DequeueLoop(token));
		}

		private async Task DequeueLoop(CancellationToken token)
        {
			while (true || !token.IsCancellationRequested)
            {
				token.ThrowIfCancellationRequested();
				if (closing) throw new Exception("UDPListener has been closed.");

				if (packetQueue.Count() > 0){
					OscPacket packet;
					packetQueue.TryDequeue(out packet);
					if(packet != null)
                        try{
                            EvaluateAddresses(packet);
                        }catch{}
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
				try{
					packet = OscPacket.GetPacket(bytes);
				}
				catch (Exception e){
					// If there is an error reading the packet, null is sent to the callback
                    Console.WriteLine("Error reading OSC bytes: " + e.Message);
				}

				if(packet != null){
                    Console.WriteLine($"Received {bytes.Length} byte of data");
                    //if reached max queue size discard exceding message
					while(packetQueue.Count >= _MAX_QUEUE_SIZE)
						packetQueue.TryDequeue(out OscPacket tmp);
					packetQueue.Enqueue(packet);
				}
				
                if (OscPacketCallback != null)
                {
                    try{
                        OscPacketCallback(packet);
                    }catch{}
                }
                
                if(!delayedAddressCallback){
                    try{
                        EvaluateAddresses(packet);
                    }catch{}
                }
            }
        }

        private void EvaluateAddresses(OscPacket packet)
        {
			if(!packet.IsBundle){
				var address = ((OscMessage)packet).Address;
                if(Utils.ValideteAddress(address)){
				    addressCallbacks.Where(x => Utils.MatchAddress(x.address,address)).ToList().ForEach(x => x.callback.Invoke(packet));
                }else{
                    Console.WriteLine("Received message address malformed");
                }
			}else{
				var bundle = (OscBundle)packet;
				bundle.Messages.ForEach(m => {
                    if(Utils.ValideteAddress(m.Address)){
                        addressCallbacks.Where(x => Utils.MatchAddress(x.address,m.Address)).ToList().ForEach(x => x.callback.Invoke(m));
                    }else{
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
		public bool AddAddress(string address, HandleOscPacket handleOscPacket){
			AddressHandler addressHandler = new AddressHandler(address,handleOscPacket);
			if(addressHandler.isValid){
				addressCallbacks.Add(addressHandler);
                Console.WriteLine($"Address Added: {address}");
            }else{
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
		/// Get single message from queue.
		/// WARNING! Removes it from address evaluation loop.
        /// WARNING! Can be NULL, require a nullcheck.
		/// </summary>
		/// <returns>message/bundle packet, can be NULL</returns>
        public OscPacket Receive()
        {
            if (closing) throw new Exception("UDPListener has been closed.");

            if (packetQueue.Count() > 0)
            {
                OscPacket packet = null;
                packetQueue.TryDequeue(out packet);
                return packet;
            }
            else
                return null;
        }

    }
}
