using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace OwlOSC
{
	public class UDPSender
	{

		private const int _MAX_PACKET_SIZE = 65507;

		public int Port
		{
			get { return _port; }
		}
		int _port;

		public string Address
		{
			get { return _address; }
		}
		string _address;

		IPEndPoint RemoteIpEndPoint;
		Socket sock;

		public UDPSender(string address, int port)
		{
			_port = port;
			_address = address;

			sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			var addresses = System.Net.Dns.GetHostAddresses(address);
			if (addresses.Length == 0) throw new Exception("Unable to find IP address for " + address);

			RemoteIpEndPoint = new IPEndPoint(addresses[0], port);
		}

		public void Send(byte[] message)
		{
			if(message.Length > _MAX_PACKET_SIZE)
				throw new Exception("Message exceeds UDP max packet size (64k)");
			sock.SendTo(message, RemoteIpEndPoint);
		}

		public void Send(OscPacket packet)
		{
			byte[] data = packet.GetBytes();
			if(data.Length > _MAX_PACKET_SIZE)
				throw new Exception("Message exceeds UDP max packet size (64k)");
			Send(data);
		}

		public void Close()
		{
			sock.Close();
		}
	}
}
