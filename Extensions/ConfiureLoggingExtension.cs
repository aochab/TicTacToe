using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicTacToe.Options;
using TicTacToeLogging;

namespace TicTacToe.Extensions
{
    public static class ConfiureLoggingExtension
    {
        public static ILoggingBuilder AddLoggingConfiguration(this ILoggingBuilder loggingBuilder,
            IConfiguration configuration)
        {
            var loggingOptions = new LoggingOptions();
            configuration.GetSection("Logging").Bind(loggingOptions);

            foreach (var provider in loggingOptions.Providers)
            {
                switch (provider.Name.ToLower())
                {
                    case "console":
                        {
                            loggingBuilder.AddConsole();
                            break;
                        }
                    case "file":
                        {
                            string filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),
                                "logs", $"TicTacToe_{System.DateTime.Now.ToString("ddMMyyHHmm")}.log");
                            loggingBuilder.AddFile(filePath, (LogLevel)provider.LogLevel);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            return loggingBuilder;
        }
    }
}
