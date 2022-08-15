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
	public delegate void HandleOscPacket(OscPacket packet);
	public delegate void HandleBytePacket(byte[] packet);

	public class UDPListener : IDisposable
	{
		public int Port { get; private set; }
		
		//object callbackLock;

		UdpClient receivingUdpClient;
		IPEndPoint RemoteIpEndPoint;

		HandleBytePacket BytePacketCallback = null;
		HandleOscPacket OscPacketCallback = null;

		ConcurrentQueue<byte[]> queue;
		//ManualResetEvent ClosingEvent;

		CancellationTokenSource cancelTokenSource;
		CancellationToken token;

		public UDPListener(int port)
		{
			Port = port;
			queue = new ConcurrentQueue<byte[]>();
			//ClosingEvent = new ManualResetEvent(false);
			//callbackLock = new object();

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

			// setup first async event
			//AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
			//receivingUdpClient.BeginReceive(callBack, null);

			//
			cancelTokenSource = new CancellationTokenSource();
			token = cancelTokenSource.Token;
			Task.Run(() => BeginListeningAsync(token));
		}

		public UDPListener(int port, HandleOscPacket callback) : this(port)
		{
			this.OscPacketCallback = callback;
		}

		public UDPListener(int port, HandleBytePacket callback) : this(port)
		{
			this.BytePacketCallback = callback;
		}

		public async Task BeginListeningAsync (CancellationToken token)
		{
			while (true || !token.IsCancellationRequested) {
				token.ThrowIfCancellationRequested ();
				if (closing) throw new Exception("UDPListener has been closed.");
				try {
					var result = await receivingUdpClient.ReceiveAsync ();
					ProcessData(result.Buffer);
				}catch (ObjectDisposedException) {
					// Ignore if disposed. This happens when closing the listener
				} catch (SocketException ex) {
					Console.WriteLine(ex.Message);
					throw;
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
					throw;
				}
			}
			Console.WriteLine("End Listenting Async");
		}

		void ProcessData(Byte[] bytes){
			// Process bytes
			if (bytes != null && bytes.Length > 0)
			{
				if (BytePacketCallback != null)
				{
					BytePacketCallback(bytes);
				}
				else if (OscPacketCallback != null)
				{
					OscPacket packet = null;
					try
					{
						packet = OscPacket.GetPacket(bytes);
					}
					catch (Exception)
					{
						// If there is an error reading the packet, null is sent to the callback
					}

					OscPacketCallback(packet);
				}
				else
				{
					queue.Enqueue(bytes);
				}
			}
		}

		/*
		void ReceiveCallback(IAsyncResult result)
		{
			Monitor.Enter(callbackLock);
			Byte[] bytes = null;

			try
			{
				bytes = receivingUdpClient.EndReceive(result, ref RemoteIpEndPoint);
			}
			catch (ObjectDisposedException e)
			{ 
				// Ignore if disposed. This happens when closing the listener
			}

			// Process bytes
			if (bytes != null && bytes.Length > 0)
			{
				if (BytePacketCallback != null)
				{
					BytePacketCallback(bytes);
				}
				else if (OscPacketCallback != null)
				{
					OscPacket packet = null;
					try
					{
						packet = OscPacket.GetPacket(bytes);
					}
					catch (Exception e)
					{
						// If there is an error reading the packet, null is sent to the callback
					}

					OscPacketCallback(packet);
				}
				else
				{
					lock (queue)
					{
						queue.Enqueue(bytes);
					}
				}
			}

			if (closing)
				ClosingEvent.Set();
			else
			{
				// Setup next async event
				AsyncCallback callBack = new AsyncCallback(ReceiveCallback);
				receivingUdpClient.BeginReceive(callBack, null);
			}
			Monitor.Exit(callbackLock);
		}

		
		public void Close()
		{
			lock (callbackLock)
			{
				ClosingEvent.Reset();
				closing = true;
				receivingUdpClient.Close();
			}
			ClosingEvent.WaitOne();
			
		}
		*/

		bool closing = false;

		public void Close(){
			closing = true;
			cancelTokenSource.Cancel();
			receivingUdpClient.Close();
		}

		public void Dispose()
		{
			this.Close();
			receivingUdpClient.Dispose();
		}

		public OscPacket Receive()
		{
			if (closing) throw new Exception("UDPListener has been closed.");

			lock (queue)
			{
				if (queue.Count() > 0)
				{
					byte[] bytes;
					queue.TryDequeue(out bytes);
					//byte[] bytes = queue.Dequeue();
					var packet = OscPacket.GetPacket(bytes);
					return packet;
				}
				else
					return null;
			}
		}

		/*
		public byte[] ReceiveBytes()
		{
			if (closing) throw new Exception("UDPListener has been closed.");

			lock (queue)
			{
				if (queue.Count() > 0)
				{
					byte[] bytes = queue.Dequeue();
					return bytes;
				}
				else
					return null;
			}
		}
		*/
		
	}
}
