using SimpleServer.Net;
using SimpleServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket.Instance.Init(); 
            Console.ReadLine();
        }
    }
}
