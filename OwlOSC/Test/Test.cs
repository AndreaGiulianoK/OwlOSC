
using System;

namespace OwlOSC{
    public class Test {

        void Main(){
            OscPacket packet = new OscMessage("/",1);
            if(packet.IsBundle){}
        }
        
    }
}