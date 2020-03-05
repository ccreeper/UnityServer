using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerBase
{
    public static class Debug
    {
        private static ILog log;

        static Debug() {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));

            log = LogManager.GetLogger(typeof(Debug));
        }
    }
}
