using Microsoft.Extensions.Configuration;

namespace Visuallies
{
    internal class ConfigManager
    {
        public IConfiguration Config { get; set; }

        public ConfigManager(string? configFilePath = null)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile(configFilePath ?? "appsettings.json",
                   optional: false, reloadOnChange: true);

            Config = builder.Build();
        }
    }
}
