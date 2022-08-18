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

		private const int _MAX_QUEUE_SIZE = 1000;

        public int Port { get; private set; }

        UdpClient receivingUdpClient;
        IPEndPoint RemoteIpEndPoint;

        HandleBytePacket BytePacketCallback = null;
        HandleOscPacket OscPacketCallback = null;

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
		/// Start Listner with immediate Byte array Callback.
		/// </summary>
		/// <param name="port">>Listening port</param>
		/// <param name="callback">Byte array received callback</param>
		/// <returns>Listner instance</returns>
        public UDPListener(int port, HandleBytePacket callback) : this(port)
        {
            this.BytePacketCallback = callback;
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

		/// <summary>
		/// Start address callback evaluation loop
		/// </summary>
		public void StartAddressEvaluationLoop(){
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
						EvaluateAddresses(packet);
				}
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
				catch (Exception){
					// If there is an error reading the packet, null is sent to the callback
				}

				if(packet != null){
					OscPacket tmp;
					while(packetQueue.Count >= _MAX_QUEUE_SIZE)
						packetQueue.TryDequeue(out tmp);
					packetQueue.Enqueue(packet);
				}
				
                if (BytePacketCallback != null)
                {
                    BytePacketCallback(bytes);
                }
                else if (OscPacketCallback != null)
                {
                    OscPacketCallback(packet);
                }
            }
        }

        private void EvaluateAddresses(OscPacket packet)
        {
			if(!packet.IsBundle){
				var address = ((OscMessage)packet).Address;
                if(Utils.ValideteAddress(address)){
				    addressCallbacks.Where(x => x.address == address).ToList().ForEach(x => x.callback.Invoke(packet));
                }else{
                    Console.WriteLine("Received message address malformed");
                }
			}else{
				var bundle = (OscBundle)packet;
				bundle.Messages.ForEach(m => {
                    if(Utils.ValideteAddress(m.Address)){
                        addressCallbacks.Where(x => x.address == m.Address).ToList().ForEach(x => x.callback.Invoke(m));
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
		/// </summary>
		/// <returns>message/bundle packet, can be NULL if malformed</returns>
        public OscPacket Receive()
        {
            if (closing) throw new Exception("UDPListener has been closed.");

            if (packetQueue.Count() > 0)
            {
                OscPacket packet;
                packetQueue.TryDequeue(out packet);
                return packet;
            }
            else
                return null;
        }

    }
}
