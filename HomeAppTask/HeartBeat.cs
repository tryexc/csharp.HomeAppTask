using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAppTask
{
    class HeartBeat
    {
        SimpleTCP.SimpleTcpClient tcpclient;
        
        public void HeartBeat_start(int port)
        {
            if (port != -1)
            {
                tcpclient = new SimpleTCP.SimpleTcpClient().Connect("127.0.0.1", port);


                while (true)
                {
                    //var timestamp = DateTime.Now.ToFileTime();
                    var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    
                    string strToSend = "HEARTBEAT HomeAppTask.exe " + timestamp.ToString();

                    tcpclient.WriteLine(strToSend);
                    Console.WriteLine(strToSend);

                    System.Threading.Thread.Sleep(900);
                }



            }
            else
            {
                while (true)
                {
                    //var timestamp = DateTime.Now.ToFileTime();
                    var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                    string strToSend = "HomeAppTask " + timestamp.ToString();
                    
                    Console.WriteLine(strToSend);
                    
                    System.Threading.Thread.Sleep(900);
                }
            }




        }




    }
}
