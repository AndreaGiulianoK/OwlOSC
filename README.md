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

+ Multiplatform menaged dll (NetStandard 2.1)
+ Send / Receive OSC messages and bundle via UDP
+ Register address callback
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

## Performance and Testing

### Speed:

Single message speed on send-receive on localhost

Speed: ~ 0.025ms (linux) / ~0.05ms (win)

*Note: for reliability insert a delay of 1ms between two consecutive message otherwise udp packet can be dropped.*

### Successful Testing:

- [x] Linux Ubuntu 20.04 (x64)
- [ ] Raspberry OS (ARM)
- [x] Windows 10 (x64)
- [ ] macOS / OS X (x64)
- [ ] Android
- [ ] iOS


## Changelog

Changelog here **[Changelog.md](https://github.com/AndreaGiulianoK/OwlOSC/blob/master/CHANGELOG.md)**


## Using The Library

### Unity3D:

Import the package.
OwlOSC is under that namespace "OwlOSC".
Look at the example scene in the folder "OwlOSC/Examples"

### .NET:

Add a reference to OwlOSC.dll in your .NET project. 
OwlOSC is under that namespace "OwlOSC".

## Examples:

### .NET: Sending a message Synchronously

	class Program
	{
		static void Main(string[] args)
		{
			var sender = new OwlOSC.UDPSender("127.0.0.1", 55555);
			OwlOSC.OscMessage message = new OwlOSC.OscMessage("/test/1", 23, 42.01f, "hello world");
			sender.Send(message);
			sender.Close();
		}
	}

This example sends an OSC message to the local machine on port 55555 containing 3 arguments: an integer with a value of 23, a floating point number with the value 42.01 and the string "hello world". If another program is listening to port 55555 it will receive the message and be able to use the data sent.

### .NET: Receiving a Message Asynchronous Manually

	class Program
	{
		static void Main(string[] args)
		{
			var listener = new OwlOSC.UDPListener(55555);
			OwlOSC.OscPacket messageReceived=null;
			while(messageReceived == null){
				messageReceived = listener.Receive();
				System.Threading.Thread.Sleep(1);
			}
			listener.Close();
			Console.WriteLine(messageReceived.ToString());
		}
	}

This shows a very simple way of waiting for incoming messages. The listener.Receive() method will check if the listener has received any new messages since it was last called. It will poll for a message every millisecond. If there is a new message that has not been returned it will assign messageReceived to point to that message. If no message has been received since the last call to Receive it will return null.
When messageReceived is pointig to a message the cycle ends, the listner is closed and the content of the message is returned to the console.

### .NET:  Receiving a Message Asynchronous by direct callback

	class Program
	{
		public void Main(string[] args)
		{
			var listener = new OwlOSC.UDPListener(localPort, (packet) => {
				Console.WriteLine(packet.ToString());
			});

			//keeps the program open until a key is pressed
			Console.WriteLine("\nPress any key to stop and exit...");
			Console.ReadKey();
			listener.Close();
		}
	}

By giving UDPListener a callback you don't have to periodically check for incoming messages. The listener will simply invoke the callback whenever a message is received. No address check is performed.

### .NET:  Receiving a Message Asynchronous by Address callback handling

	class Program
	{
		public void Main(string[] args)
		{
			var listener = new OwlOSC.UDPListener(localPort);
			//register a callback for specific address, only messages with the corresponding address will invoke the callback
			bool address = listener.AddAddress("/test", (packet) => {
				Console.WriteLine("Address: " + packet.ToString());
			});

			//keeps the program open until a key is pressed
			Console.WriteLine("\nPress any key to stop and exit...");
			Console.ReadKey();
			listener.Close();
		}
	}

By registering a callback to UDPListener the listener will invoke the callback whenever a message with the matching address is received.

### UNITY:  Example Send/Receive script

	using System.Collections;
	using UnityEngine;
	using OwlOSC;

	public class OwlOscTest : MonoBehaviour
	{
		UDPSender sender;
		UDPListener listener;

		private void Start() {
			//Instantiate Sender and send a message
			sender = new UDPSender("127.0.0.1",55555);
			sender.Send(new OscMessage("/test", 42, "hello"));
			//Instantiate Listener and register address
			listener = new UDPListener(55555);
			listener.AddAddress("/test",(packet) => {
				Debug.Log(packet.ToString());
			});
			listener.StartAddressEvaluationLoop();
			//Optionally you read directly in a coroutine
			StartCoroutine(ReadLoop());
		}

		private void OnDestroy() {
			StopAllCoroutines();
			sender.Close();
			listener.Close();
		}

		IEnumerator ReadLoop(){
			while(true){
				var packet = listener.Receive();
				if(packet != null)
				Debug.Log(packet.ToString());
				yield return new WaitForEndOfFrame();
			}
		}
	}


## Contribute

I would love to get some feedback. Use the Issue tracker on Github to send bug reports and feature requests, or just if you have something to say about the project. If you have code changes that you would like to have integrated into the main repository, send me a pull request or a patch. I will try my best to integrate them and make sure OwlOSC improves and matures.

## TO DO:

 - [x] Upgrade UDP to Async Operation
 - [x] Add more practical handling and discrimination of messages and bundles
 - [x] Add receiveng message address check and relative event handling
 - [ ] Add message values Getter with nullcheck
 - [x] Release Dll
 - [x] Unity Test
 - [ ] Unity Interface
 - [ ] Unity Examples