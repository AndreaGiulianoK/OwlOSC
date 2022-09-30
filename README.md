# OwlOSC

Small ***C# menaged library*** that implement ***Open Sound Control (OSC 1.0)*** communication protocol with ***multithreaded queued message receiver with thread safe callback and address pattern match***  for ***UNITY3D*** and any ***NetStandard*** (Net Framework / NetCore) project.

*As a managed library it runs on any DOT NET supported platform.*
*As a Unity 3D plugin it runs only in x86_64 Standalone (Windows,Linux,Mac) and on mobile platforms (Android, iOS); **no support for WebGL** (due to the security restrictions of the webgl specifications).*

Developers: ***Andrea Giuliano***

(OSC operability based upon **[ValdemarOrn SharpOSC](https://github.com/ValdemarOrn/SharpOSC)**)


# Contents
- [License](#license) 
- [Download](#download)
- [Features](#features)
- [Supported Types](#supported-types)
- [Supported Address Pattern](#supported-address-pattern)
- [Performance and Testing](#performance-and-testing)
- [Changelog](#changelog)
- [Build](#build)
- [Use Examples](#use-examples)
- [Contribute](#contribute)
- [TO DO](#to-do)

## License

OwlOSC is licensed under the MIT license.

See [License.md](https://github.com/AndreaGiulianoK/OwlOSC/blob/master/LICENSE.md)


## Download

Compiled library and unity package here: **[Releases](https://github.com/AndreaGiulianoK/OwlOSC/releases)**


## Features

+ Menaged dll (NetStandard 2.1)
+ Send / Receive OSC messages and bundle via UDP
+ Receive OSC message in background Thread
+ Thread Safe received message queue
+ Pattern address callback with crossed comparation between message and registered path
+ Partial support for pattern wildcards (only '*')
+ OSC values converted from and to Net objects
+ Unity3D Examples
+ **Unity3D Main Thread safe async callbacks option** (needed to instantiate objects in main render thread)


## Supported Types

[The following OSC types (OSC Spec 1.0)](https://opensoundcontrol.stanford.edu/spec-1_0.html) are supported:

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

## Supported Address Pattern

Only * charater is supported. Path comparation is performed between received message prefix and  callback address path.
Every message prefix and every callback pattern must pass address validation regex:

```
^\/$|^\/([a-zA-Z0-9\/\*\[\]-]*)([a-zA-Z0-9\*\]])$
```

**Supported wildcard:**

- __*__

**NO support for other wildcards:**

- ? (single character)
- [ - ] (range)

Callback Path Example:

- '*' -> matches any message prefix (fast direct evaluation without regex)
- '/' -> matches only root '/'
- '/*' -> matches any message prefix
- '/test' -> matches only '/test'
- '/test/* -> matches any prefix in test ('/test/1','/test/a/sub', etc)
- '/test/*ub' -> matches any prefix in test that has any subpath ending with 'ub' ('/test/sub','/test/pub',etc)
- '/test/*/sub' -> matches any prefix in test that has 'sub' as second subpath ('/test/a/sub','/test/1/sub', etc)

[OSC spec 1.0 Address path reference](https://ccrma.stanford.edu/groups/osc/spec-1_0-examples.html#OSCaddress)

## Performance and Testing

### Speed:

Single message speed on send-receive on localhost

Speed: ~ 0.025ms (linux) / ~0.05ms (win)

*Note: for reliability insert a delay of 1ms between two consecutive message reads otherwise udp packet can be dropped with mass receiving.*

### Successful Testing:

### Linux
- [x] Console - Linux Ubuntu 20.04 (x64)
- [x] Unity3D (Mono) - Linux Ubuntu 20.04 (x64)
- [x] Unity3D (IL2CPP) - Linux Ubuntu 20.04 (x64)

### Windows
- [x] Console - Windows 10 (x64)
- [x] Unity3D (Mono) - Windows 10 (x64)
- [ ] Unity3D (IL2CPP) - Windows 10 (x64)

### OS X
- [ ] Unity (Mono) - OS X (x64)

### Raspberry OS
- [ ] Console - Raspberry OS (ARM)

### Android
- [ ] Unity3D (Mono)
- [ ] Unity3D (IL2CPP)

### iOS
- [ ] Unity3D (IL2CPP)


## Changelog

Changelog here **[Changelog.md](https://github.com/AndreaGiulianoK/OwlOSC/blob/master/CHANGELOG.md)**

## Build

### Library

Tips for compiling the Library with VScode and dotnet.

Simple: compile in Debug for netstandard2.1, netcoreapp3.1, net4.7.1
```
cd OwlOSC
dotnet build
```
Complex: specify release configuration and target framework
```
cd OwlOSC
dotnet build -c Release -f {framework}
```
Where `{framework}` is the target framework:
- netcoreapp3.1
- netstandard2.1

### Test Application

Tips for compiling the test console program with VScode and dotnet.
```
cd OwlOsc.Test
dotnet publish -c Release -r {platform} -p:PublishSingleFile=true --self-contained false
```
Or for single container executable with all system dll
```
cd OwlOsc.Test
dotnet publish -c Release -r {platform} -p:PublishSingleFile=true --self-contained true
```
Where `{platform}` is the target platform os and CPU architecture RID:
- win-x64
- win-x86
- win-arm
- win-arm64
- linux-x64
- linux-musl-x64
- linux-arm
- linux-arm64
- osx-x64
- ios-arm64
- android-arm64

## Using The Library

### Unity3D:

Import the package located in *[Release Section](https://github.com/AndreaGiulianoK/OwlOSC/releases)**.
OwlOSC is under the namespace "OwlOSC".
Look at the example scene "OwlOSC/OwlOscTest.unity"

### .NET:

Add a reference to OwlOSC.dll in your .NET project.
OwlOSC is under the namespace "OwlOSC".

## Use Examples:

### .NET

#### .NET: Sending a message Synchronously

	class Program
	{
		static void Main(string[] args)
		{
			using(var sender = new OwlOSC.UDPSender("127.0.0.1", 55555))
			{
				OwlOSC.OscMessage message = new OwlOSC.OscMessage("/test/1", 23, 42.01f, "hello world");
				sender.Send(message);
			}

		}
	}

This example sends an OSC message to the local machine on port 55555 containing 3 arguments: an integer with a value of 23, a floating point number with the value 42.01 and the string "hello world". If another program is listening to port 55555 it will receive the message and be able to use the data sent.

#### .NET:  Receiving a Message Synchronously with Address callback handling

	class Program
	{
		public void Main(string[] args)
		{
			//create a listener with immediate sync callback
			var listener = new OwlOSC.UDPListener(localPort,false);
			//register a callback for specific address, only messages with the corresponding address will invoke the callback
			bool address = listener.AddAddress("/test", (packet) => {
				Console.WriteLine("Message: " + packet.ToString());
			});

			//keeps the program open until a key is pressed
			Console.WriteLine("\nPress any key to stop and exit...");
			Console.ReadKey();
			listener.Dispose();
		}
	}

By registering a callback to UDPListener the listener will invoke the callback whenever a message with the matching address is received. By creating an UDPListener with `queueMessages = false` the listener will evaluate the address path and invoke the callback immediatly. By creating an UDPListener with `queueMessages = true` the listener will enqueue the message and must be manually readed from the queue.


#### .NET: Receiving a Message Manually

	class Program
	{
		static void Main(string[] args)
		{
			//create a listener with message queue and no addreses callbacks evaluation loop
			using(var listener = new OwlOSC.UDPListener(55555))
			{
				//register a callback for specific address, only messages with the corresponding address will invoke the callback
				bool address = listener.AddAddress("/test", (packet) => {
					Console.WriteLine("Message: " + packet.ToString());
				});
				OwlOSC.OscPacket messageReceived=null;
				while(messageReceived == null)
				{
					//get a message from the queue and NOT evaluate address path and callbacks
					messageReceived = listener.ReadQueuedMessage(false);
					System.Threading.Thread.Sleep(1);
				}
				Console.WriteLine(messageReceived.ToString());
			}
		}
	}

This shows a way of waiting for a single incoming messages.

The listener was created by disabling the synchronous match by assigning `queueMessages = true` and thus allowing the routing of messages to the concurrent Queue. In this context it is obligatory to manually read the messages from the queue with the method `ReadQueuedMessage` or `ReadAllQueuedMessages` and choose whether to invoke the evaluation of the address and consequent callback or simply obtain the message by setting `evaluateAddressCallback = false`.

In this example when `messageReceived` is pointig to a valid message the cycle ends, the Listener is closed and the content of the message is returned to the console but the callback relative for the registered address will never be invoked.

### UNITY

#### UNITY:  Example Send script

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using OwlOSC;

	public class SimpleSender : MonoBehaviour
	{
		public string remoteHost = "127.0.0.1";
		public int remotePort = 55555;
		public string prefix = "/test";

		private void OnEnable() {
			Send();
		}

		[ContextMenu("Send Message")]
		public void Send(){
			using(var sender = new UDPSender(remoteHost,remotePort)){
				sender.Send(new OscMessage(prefix, Random.Range(0,float.MaxValue), "hello"));
			}
		}
	}

#### UNITY:  Example Receive script

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using OwlOSC;

	public class SimpleListener : MonoBehaviour
	{
		public int localPort = 55555;
		UDPListener listener;
		
		private void OnEnable() {
			//Instantiate simple listener
			listener = new UDPListener(localPort);
			//register address path callback
			listener.AddAddress("/*",(packet) => {
				Debug.Log(packet.ToString());
			});
		}

		private void Update() {
			//read every frame all messages in queue
			listener.ReadAllQueuedMessages();
		}

		private void OnDisable() {
			listener.Dispose();
		}
	}


In unity it is possible to create new Unity GameObjects only from the main thread. Since UDPListener is multithreaded if you receive the callback directly it is not possible to instantiate new objects.
it is therefore necessary to read manually from a coroutine or the `Update` function (which acts on the main thread). In this way it is possible to receive all messages and perform "heavy" actions in the Unity Render thread without affecting the speed of reading messages.


## Contribute

I would love to get some feedback. Use the Issue tracker on Github to send bug reports and feature requests, or just if you have something to say about the project. If you have code changes that you would like to have integrated into the main repository, send me a pull request or a patch. I will try my best to integrate them and make sure OwlOSC improves and matures.

## TO DO:

 - [x] Upgrade UDP to Async Operation
 - [x] Add more practical handling and discrimination of messages and bundles
 - [x] Add receiveng message address check and relative event handling
 - [x] Support simple Wildcard in match address
 - [ ] ~~Add message values Getter with nullcheck~~ (useless)
 - [x] Data type description in `ToString` method
 - [x] Release Dll
 - [x] Unity Test
 - [ ] Unity Interface
 - [ ] Unity Examples

## TO TEST:
 - [x] Send/Receive/Print all supported data type
 - [ ] Test all Platforms