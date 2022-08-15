# OwlOSC

Small library that implement ***Open Sound Control (OSC)*** communication protocol for ***UNITY3D*** and any NetStandard (Net Framework / NetCore) project.

Developers: ***Andrea Giuliano***
OSC operability based upon **[ValdemarOrn SharpOSC](https://github.com/ValdemarOrn/SharpOSC)**


## License

OwlOSC is licensed under the MIT license.

See [License.md](https://github.com/AndreaGiulianoK/OwlOSC/blob/master/LICENSE.md)


## Download


Compiled library and unity package here: **[Releases](https://github.com/AndreaGiulianoK/OwlOSC/releases)**


## Features

+ Send / Receive OSC messages and bundle via UDP
+ OSC values converted from and to Net objects
+ Unity3D interface and utilities


## Supported Types


[The following OSC types](http://opensoundcontrol.org/spec-1_0) are supported:

* i	- int32 (System.Int32)
* f	- float32 (System.Single)
* s	- OSC-string (System.String)
* b	- OSC-blob (System.Byte[])
* h	- 64 bit big-endian two's complement integer (System.Int64)
* t	- OSC-timetag (System.UInt64 / OwlOSC.Timetag)
* d	- 64 bit ("double") IEEE 754 floating point number (System.Double)
* S	- Alternate type represented as an OSC-string (for example, for systems that differentiate "symbols" from "strings") (OwlOSC.Symbol)
* c	- an ascii character, sent as 32 bits (System.Char)
* r	- 32 bit RGBA color (OwlOSC.RGBA)
* m	- 4 byte MIDI message. Bytes from MSB to LSB are: port id, status byte, data1, data2 (OwlOSC.Midi)
* T	- True. No bytes are allocated in the argument data. (System.Boolean)
* F	- False. No bytes are allocated in the argument data. (System.Boolean)
* N	- Nil. No bytes are allocated in the argument data. (null)
* I	- Infinitum. No bytes are allocated in the argument data. (Double.PositiveInfinity)
* [	- Indicates the beginning of an array. The tags following are for data in the Array until a close brace tag is reached. (System.Object[] / List\<object\>)
* ]	- Indicates the end of an array.

(Note that nested arrays (arrays within arrays) are not supported, the OSC specification is unclear about whether that it is even allowed)

## Changelog

Changelog here (Changelog.md)[https://github.com/AndreaGiulianoK/OwlOSC/blob/master/CHANGELOG.md]


## Using The Library

### Unity3D:

Import the package.
OwlOSC is under that namespace "OwlOSC".
Look at the example scene in the folder "OwlOSC/Examples"

### .NET:

Add a reference to OwlOSC.dll in your .NET project. 
OwlOSC is under that namespace "OwlOSC".

## Examples:

### .NET: Sending a message

	class Program
	{
		static void Main(string[] args)
		{
			var message = new OwlOSC.OscMessage("/test/1", 23, 42.01f, "hello world");
			var sender = new OwlOSC.UDPSender("127.0.0.1", 55555);
			sender.Send(message);
		}
	}

This example sends an OSC message to the local machine on port 55555 containing 3 arguments: an integer with a value of 23, a floating point number with the value 42.01 and the string "hello world". If another program is listening to port 55555 it will receive the message and be able to use the data sent.

### .NET: Receiving a Message (Synchronous)

	class Program
	{
		static void Main(string[] args)
		{
			var listener = new UDPListener(55555);
			OscMessage messageReceived = null;
			while (messageReceived == null)
			{
				messageReceived = (OscMessage)listener.Receive();
				Thread.Sleep(1);
			}
			Console.WriteLine("Received a message!");
		}
	}

This shows a very simple way of waiting for incoming messages. The listener.Receive() method will check if the listener has received any new messages since it was last called. It will poll for a message every millisecond. If there is a new message that has not been returned it will assign messageReceived to point to that message. If no message has been received since the last call to Receive it will return null.

### .NET:  Receiving a Message (Asynchronous)

	class Program
	{
		public void Main(string[] args)
		{
			// The cabllback function
			HandleOscPacket callback = delegate(OscPacket packet)
			{
				var messageReceived = (OscMessage)packet;
				Console.WriteLine("Received a message!");
			};

			var listener = new UDPListener(55555, callback);

			Console.WriteLine("Press enter to stop");
			Console.ReadLine();
			listener.Close();
		}
	}

By giving UDPListener a callback you don't have to periodically check for incoming messages. The listener will simply invoke the callback whenever a message is received. You are free to implement any code you need inside the callback.

## Contribute

I would love to get some feedback. Use the Issue tracker on Github to send bug reports and feature requests, or just if you have something to say about the project. If you have code changes that you would like to have integrated into the main repository, send me a pull request or a patch. I will try my best to integrate them and make sure OwlOSC improves and matures.

### TO DO:

 - [ ] Upgrade UDP to Async Operation
 - [ ] Add more practical handling and discrimination of messages and bundles
 - [ ] Add message values Getter with nullcheck
 - [ ] Unity Interface
 - [ ] Unity Example