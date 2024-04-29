
using Microsoft.Extensions.Configuration;
using Serilog;
using Sensia.HCC2.SDK.Classes;
using Sensia.hcc2sdkcs;

namespace csapp
{
    class Program
    {
        public static IConfigurationRoot appConfig;
        private static DB db;
        private static EventWaitHandle ready_ev;
        

        public static async Task<int> Main()
        {
            //
            // Setup the logger
            //
            ConfigurationBuilder config = new ConfigurationBuilder();
            BuildConfig(config);

            appConfig = config.Build();

            // Set up Serilog, use configuration source
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(appConfig)
                .Enrich.With(new LogLevelAbbreviationEnricher())    // ensure the correct log level names are used
                .Enrich.WithProperty("Version", "0")
                .Enrich.WithProperty("Logger", "CSSDK") // Default logging context (see appsettings.json)
                .CreateLogger();
            
            Log.Information("HCC2 Application Started!");
            //
            // Create the control queue
            //
            Queue<KeyValuePair<string, RealtimeControl>> controlQueue = new Queue<KeyValuePair<string, RealtimeControl>>();
            //
            // create the DB
            //
            db = new DB(controlQueue);
            // 
            // create the locking event
            ready_ev = new EventWaitHandle(false, EventResetMode.AutoReset);
            //
            // Start the Modbus Client
            //
            Task<int> mct = Task.Run(() => {
                var mc = new HCC2Interface(appConfig, db, ready_ev);
                mc.StartClient();
                return 0;
             }); 

            Log.Information("Main: Engine started!");
            //
            // Start the App
            //
            Task<int> ap = Task.Run(() => {
                var app = new App(appConfig, db, ready_ev);
                app.StartApp();
                return 0;
             });    

            Log.Information("Main: App started!");

            await mct;
            await ap;
            
            Log.Information("Main: App ended!");
            return 0; 
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}