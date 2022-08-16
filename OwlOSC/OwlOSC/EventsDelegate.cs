using System;

namespace OwlOSC {
    	

    public delegate void HandleOscPacket(OscPacket packet);
	public delegate void HandleBytePacket(byte[] packet); 

	public delegate void HandleAddress(AddressHandler addressHandler);

	public struct AddressHandler{
		public string address {get; private set;}
		public HandleOscPacket callback {get; private set;}
		public bool isValid {get; private set;}

		public AddressHandler(string address, HandleOscPacket callback){
			this.address = address;
			this.callback = callback;
			this.isValid = Utils.ValideteAddress(address);
		}
	}
      
}