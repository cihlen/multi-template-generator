﻿//using System;
//using System.Diagnostics;
//using System.IO;
//using Serilog;

//namespace MultiTemplateGenerator.UI.Helpers
//{
//    public static class LoggerHelper
//    {
//        public static ILogger CreateFileLogger()
//        {
//            var logPath = "Multi-Template_Generator_Log.txt";
//            try
//            {
//                logPath = Path.Combine(Path.GetTempPath(), "Multi-Template_Generator_Log.txt");
//            }
//            catch (Exception e)
//            {
//                Trace.WriteLine(e);
//            }

//            ILogger logger = new LoggerConfiguration()
//                //.Enrich.FromLogContext()
//                .WriteTo.File(logPath, restrictedToMinimumLevel:
//                    Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
//                .CreateLogger();

//            return logger;
//        }
//    }
//}
