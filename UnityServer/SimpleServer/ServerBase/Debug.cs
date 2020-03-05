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

        public static void Log(object message) {
            log.Debug(message);
        }

        public static void Log(string format,params object[] args)
        {
            log.DebugFormat(format,args);
        }

        public static void LogInfo(object message)
        {
            log.Info(message);
        }

        public static void LogInfo(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        public static void LogWarn(object message)
        {
            log.Warn(message);
        }

        public static void LogWarn(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        public static void LogError(object message)
        {
            log.Error(message);
        }

        public static void LogError(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        public static void LogFatal(object message)
        {
            log.Fatal(message);
        }

        public static void LogFatal(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }
    }
}
