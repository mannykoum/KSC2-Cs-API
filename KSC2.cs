using System;
using System.IO.Ports;


namespace KSC2
{
    public class KSC2
    {
        public SerialPort ComPort;
        private long _SN;
        public string COM;


        
        /* Constructors */
        public KSC2() : this(findSerial())
        {
        }
        public KSC2(string com)
        {
            COM = com;
            // Create a new SerialPort object with default settings.
            ComPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            ComPort.PortName = this.COM;
            ComPort.BaudRate = 57600;
            ComPort.Parity = Parity.None;
            ComPort.DataBits = 8;
            ComPort.StopBits = StopBits.One;


        } 

        public static string findSerial()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                Console.Error.WriteLine("Error in findSerial()");
                return "";
            }
            return ports[0];
        }
    }
}