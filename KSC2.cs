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
        private SerialPort ComPort;
        public string SN;
        public string COM;
        public string[] Valids = new string[] { "SN", "COUPLING", "SHIELD", 
                                "MODE", "FILTER", "FC", "POSTGAIN", "PREGAIN",
                                "EXC", "EXCTYPE", "SENSE", "COMPFILT",
                                "COMPFILTFC", "COMPFILTQ", "INOVLD", "OUTOVLD",
                                "OUTOVLDLIM"};
        public string Save = "SAVE";

        
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
            } else if (cmd == Save) {
                ComPort.WriteLine(cmd);
                verify = ComPort.ReadLine();
                verify = Regex.Replace(verify, @"\s",""); // clean buffer junk
                return (verify == "DONE");
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
         * params:  int channel: optional (absence will set both channels)
         *          string coupling
         *          string shield
         *          string mode
         * returns true on success
         */
        public void configure(
            int channel, string coupling, string shield, string mode)
        {
            if (!set(channel, "COUPLING", coupling))
                error("COMMUNICATION ERROR: COUPLING NOT VERIFIED");
            if (!set(channel, "SHIELD", shield))
                error("COMMUNICATION ERROR: SHIELD NOT VERIFIED");
            if (!set(channel, "MODE", mode))
                error("COMMUNICATION ERROR: MODE NOT VERIFIED");

            return;
        }
        public void configure(string coupling, string shield, string mode)
        {
            configure(1, coupling, shield, mode);
            configure(2, coupling, shield, mode);
            
            return;
        }

        /* filter */
        public void filter(int channel, int freq_cut, string type)
        {
            // Rounding to the nearest multiple of 500 Hz
            decimal intrmd = Convert.ToDecimal(freq_cut)/500;
            freq_cut = (int)Math.Round(intrmd, MidpointRounding.AwayFromZero);
            freq_cut *= 500;
            if (freq_cut < 500)
                freq_cut = 500;
            Console.WriteLine("Cutoff frequency rounded to {0} Hz", freq_cut);
            if (!set(channel, "FC", freq_cut.ToString()))
                error("COMMUNICATION ERROR: FC NOT VERIFIED");
            if (!set(channel, "FILTER", type))
                error("COMMUNICATION ERROR: FILTER NOT VERIFIED");

            return;  
        }
        public void filter(int freq_cut, string type)
        {
            filter(1, freq_cut, type);
            filter(2, freq_cut, type);

            return;  
        }

        /* excitation */
        public void excitation
        (int channel, double voltage, string type, string sense)
        {
            type = type.ToUpper();
            sense = sense.ToUpper();

            decimal intrmd = Convert.ToDecimal(voltage/0.00125);
            voltage = (double)Math.Round(intrmd,MidpointRounding.AwayFromZero);
            voltage *= 0.00125; 
            Console.WriteLine("Excitation voltage rounded to {0} V", voltage);
            if (!set(channel, "EXC", voltage.ToString()))
                error("COMMUNICATION ERROR: EXC NOT VERIFIED");
            if (!set(channel, "EXCTYPE", type))
                error("COMMUNICATION ERROR: EXCTYPE NOT VERIFIED");
            if (!set(channel, "SENSE", sense))
                error("COMMUNICATION ERROR: SENSE NOT VERIFIED");
            
            return;
        }
        public void excitation(double voltage, string type, string sense)
        {
            excitation(1, voltage, type, sense);
            excitation(2, voltage, type, sense);

            return;  
        }

        /* cavity compensation */
        public void cavityComp(int channel, string onoff, params int[] argv)
        {
            if (argv.Length > 2) {
                error("Too many arguments.");
                return;
            }
            onoff = onoff.ToUpper();

            if (onoff == "ON") {
                int compfilt_fc = argv[0];
                int compfilt_q = argv[1];

                if (!set(channel, "COMPFILT", onoff))
                    error("COMMUNICATION ERROR: COMPFILT NOT VERIFIED");
                if (!set(channel, "COMPFILTFC", compfilt_fc.ToString()))
                    error("COMMUNICATION ERROR: COMPFILTFC NOT VERIFIED");
                if (!set(channel, "COMPFILTQ", compfilt_q.ToString()))
                    error("COMMUNICATION ERROR: COMPFILTQ NOT VERIFIED");
            } else if (onoff == "OFF") {
                if (!set(channel, "COMPFILT", onoff))
                    error("COMMUNICATION ERROR: COMPFILT NOT VERIFIED");                
            } else {
                error("Incorrect usage of cavityComp().");
            }

            return;
        }
        public void cavityComp(string onoff, params int[] argv)
        {
            cavityComp(1, onoff, argv);
            cavityComp(2, onoff, argv);
        }

        /* 
         * pregain 
         * set the pregain (automatically sets any number to the closest
         * power of 2)
         * params: 
         * channel: channel to be modified (both if omitted)
         * gain: the value for the pregain (max 128)
         */
        public void pregain(int channel, int gain)
        {
            int hi = (int) Math.Pow(2, (Math.Ceiling(Math.Log(gain, 2)))); 
            // there is a faster way using bitwise OR
            int lo = hi/2;

            if (Math.Abs(hi-gain) <= Math.Abs(lo-gain)) {
                gain = hi;
            } else {
                gain = lo;
            }

            if (gain > 128)
                gain = 128;

            if (!set(channel, "PREGAIN", gain.ToString()))
                error("COMMUNICATION ERROR: PREGAIN NOT VERIFIED");
            
            return;
        }
        public void pregain(int gain)
        {
            pregain(1, gain);
            pregain(2, gain);
        }
        
        /* 
         * postgain 
         * set the postgain 
         * params: 
         * channel: channel to be modified (both if omitted)
         * gain: the value for the postgain (max 16)
         */
        public void postgain(int channel, double gain)
        {
            decimal intrmd = Convert.ToDecimal(gain/0.0125);
            gain = (double)Math.Round(intrmd,MidpointRounding.AwayFromZero);
            gain *= 0.0125; 

            if (gain > 16)
                gain = 16;

            if (!set(channel, "POSTGAIN", gain.ToString()))
                error("COMMUNICATION ERROR: POSTGAIN NOT VERIFIED");
            
            return;
        }
        public void postgain(double gain)
        {
            postgain(1, gain);
            postgain(2, gain);
        }

        public void setOvldLim(int channel, double limit)
        {
            decimal intrmd = Convert.ToDecimal(limit/0.1);
            limit = (double)Math.Round(intrmd,MidpointRounding.AwayFromZero);
            limit *= 0.1;

            if (limit > 10.2) {
                limit = 10.2;
            } else if (limit < 0.1) {
                limit = 0.1;
            }

            if (!set(channel, "OUTOVLDLIM", limit.ToString()))
                error("COMMUNICATION ERROR: OUTOVLDLIM NOT VERIFIED");

            return;
        }
        public void setOvldLim(double limit)
        {
            setOvldLim(1, limit);
            setOvldLim(2, limit);
        }

        /* Save the current configuration */
        public void save()
        {
            set(0, "SAVE", "");
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
        public string[] get(string cmd)
        {
            string[] ans = new string[2];
            ans[0] = get(1, cmd);
            ans[0] = Regex.Replace(ans[0], @"\s","");
            ans[1] = get(2, cmd);
            ans[1] = Regex.Replace(ans[1], @"\s","");
            return ans;
        }

        public string ovldUpdate(int channel, bool in_or_not_out)
        {
            string cmd;
            if (in_or_not_out) {
                cmd = "INOVLD";
            } else {
                cmd = "OUTOVLD";
            }
            return get(channel, cmd);
        }
        public string ovldInUpdate(int channel)
        {
            return ovldUpdate(channel, true);
        }
        public string ovldOutUpdate(int channel)
        {
            return ovldUpdate(channel, false);
        }
        /* 
         * accessor method for all overload values
         * returns array of strings with the following order
         * [ovldIn chan 1, ovldIn chan 2, ovldOut chan 1, ovldOut chan 2]
         */
        public string[] getAllOvld()
        {
            string[] arr = new string[4];
            arr[0] = ovldInUpdate(1);
            arr[1] = ovldInUpdate(2);
            arr[2] = ovldOutUpdate(1);
            arr[3] = ovldOutUpdate(2);
            return arr;
        }

        /* function to query and print all the settings */
        public void printAll()
        {
            string[] arr;
            string str = "\t KSC-2 settings\n";
            str += String.Format("{0,10}: {1,-10}, {2,-10}\n",
                      "Channel", 1, 2);
            str += "----------------------------------\n";
            foreach (string cmd in Valids)
            {
                arr = get(cmd);
                str += String.Format("{0,10}: {1,-10}, {2,-10}\n",
                      cmd, arr[0], arr[1]);
            }
            Console.WriteLine(str);
        }

        /* function to query and get all the settings */
        public string[,] getAll()
        {
            string[,] arr = new string[Valids.Length,3];
            int i = 0;
            foreach (string cmd in Valids)
            {
                arr[i,0] = cmd;
                arr[i,1] = get(1, cmd);
                arr[i,2] = get(2, cmd);
                i++;
            }
            return arr;
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