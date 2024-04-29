using Sensia.HCC2.SDK.Lib;
using Sensia.HCC2.SDK.Classes;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace csapp
{
    public class App
    {
        private IConfigurationRoot appConfig;
        private EventWaitHandle ready_ev;
        private DB db;

        public App(IConfigurationRoot appConfig, DB db, EventWaitHandle ready_ev)
        {
            this.appConfig = appConfig;
            this.db = db;
            this.ready_ev = ready_ev;
        }
        public void StartApp()
        {
            string file_name = GenConfiguration.AppConfigFile;
            var appcfg = MiscFuncs.ReadAppConfigFile(file_name);
            Log.ForContext("Logger", "CSApp").Information("App: Version: " + appcfg.Version);

            while (true)
            {
                //
                // Wait for the engine to be ready
                //
                ready_ev.WaitOne();

                RealtimeData value1 = db.GetValue("cpu_temp");
                if (value1 == null)
                {
                    Log.Warning("DB is not ready. var1 does not exist. Wait next scan");
                    continue;
                }
                float v1 = (float) value1.Value;
                QualityEnum q1 = value1.Quality;

                RealtimeData value2 = db.GetValue("cpu_usage");
                if (value2 == null)
                {
                    Log.Warning("DB is not ready. var2 does not exist. Wait next scan");
                    continue;
                }
                float v2 = (float) value2.Value;
                QualityEnum q2 = value2.Quality;
                
                RealtimeData value3 = db.GetValue("mem_percentage_used");
                if (value3 == null)
                {
                    Log.Warning("DB is not ready. var3 does not exist. Wait next scan");
                    continue;
                }
                float v3 = (float) value3.Value;
                QualityEnum q3 = value3.Quality;
                
                RealtimeData value4 = db.GetValue("local_time_second");
                if (value4 == null)
                {
                    Log.Warning("DB is not ready. var4 does not exist. Wait next scan");
                    continue;
                }
                uint v4 = (uint) value4.Value;
                QualityEnum q4 = value4.Quality;


                if ((q1 == QualityEnum.ok) && (q2 == QualityEnum.ok) && (q3 == QualityEnum.ok) && (q4 == QualityEnum.ok))
                {
                    float result = v1 - 273.15f;

                    db.SetValue("result", (object) result, QualityEnum.ok);
                }
            }
            Log.Information("App loop ended.");
        }
    }
}