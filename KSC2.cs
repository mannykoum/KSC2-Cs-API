/** Class for the KSC2
 * Standalone class. Includes API and error handling.
 * @author: Emmanuel Koumandakis (emmanuel@kulite.com)
 */

using System;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;

namespace KSC2
{
    public class KSC2
    {
        public SerialPort ComPort;
        public string SN;
        public string COM;
        public string[] Valids = new string[] { "SN", "COUPLING", "SHIELD", 
                                "MODE", "FILTER", "FC", "POSTGAIN", "PREGAIN",
                                "EXC", "EXCTYPE", "SENSE", "COMPFILT",
                                "COMPFILTFC", "COMPFILTQ", "INOVLD", "OUTOVLD",
                                "OUTOVLDLIM"};
        public string save = "SAVE";

        
        /* Constructors */
        public KSC2() : this(findSerial()) {}
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

            ComPort.Open();
            // Retrieve serial number
            ComPort.WriteLine("SN?");
            SN = ComPort.ReadLine();

        } 

        /* Destructor */
        ~KSC2() 
        {
            ComPort.Close();
        }

        /* Methods to set attributes/settings */

        /* general set method to set the KSC2 
         * individual settings on a channel
         * returns true on success, false on com error 
         */
        public bool set(int channel, string cmd, string param)
        {
            string verify = "";
            string chan = channel.ToString();
            cmd = cmd.ToUpper();
            param = param.ToUpper();
            if (Valids.Contains(cmd) && channel>0 && channel<3)
            {
                ComPort.WriteLine(String.Format(
                    "{0}:{1} = {2}", channel.ToString(), cmd, param));
                verify = ComPort.ReadLine();
                verify = Regex.Replace(verify, @"\s",""); // clean buffer junk
            } else {
                error("Incorrect usage of set().");
            }
            return (param == verify);
        }

        /* same as set for both channels */
        public bool set(string cmd, string param) 
        {
            if (set(1, cmd, param) && set(2, cmd, param))
                return true;
            return false;
        }

        /* configure method 
         * params: 
         * returns true on success
         */
        public void configure(int channel, string coupling, string mode)
        {
            coupling = coupling.ToUpper();
            mode = mode.ToUpper();
            string chan = channel.ToString();

            return;
        }

        /* Methods to get attributes/settings */
        public string get(int channel, string cmd) 
        {
            cmd = cmd.ToUpper();
            if (Valids.Contains(cmd) && channel>0 && channel<3)
            {
                ComPort.WriteLine(
                    String.Format("{0}:{1}?", channel.ToString(), cmd));
                return ComPort.ReadLine();
            } else {
                error("Incorrect usage of get().");
                return "";
            }
        }
        /* Helper functions */

        /* function to automatically find the COM port */
        public static string findSerial()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                error("Error in findSerial()");
                return "";
            }
            return ports[0];
        }

        /* print error function */
        public static void error(string err)
        {
            Console.Error.WriteLine(err);
        }
    }
}