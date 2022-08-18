
using System;

namespace OwlOSC{
    public class Test {

        void Main(){
            OscBundle bundle = new OscBundle();
            bundle.Messages.Add(new OscMessage("/"));
            OscMessage message = new OscMessage("/");
            message.Arguments.Add(5);
        }
        
    }
}