using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.App.Services.Data
{
    public class LogService
    {
        private readonly HttpClient DataClient;

        public LogService(HttpClient dataClient)
        {
            this.DataClient = dataClient;
        }

        public async Task<bool> RegisterLog(Log log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.Id) ||
                string.IsNullOrWhiteSpace(log.Message))
                return false;

            try
            {
                this.DisplayNewLog(log);

                var response = await this.DataClient.PostAsJsonAsync("api/log", log);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                Log registeredLog = await response.Content.ReadFromJsonAsync<Log>();

                if (registeredLog == null)
                    this.ShowErrorLog(log);

                return true;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                this.ShowErrorLog(log);
                return false;
            }
        }

        private void DisplayNewLog(Log log)
        {
            Console.Error.WriteLine(log.ToString());
        }

        private void ShowErrorLog(Log log)
        {
            Console.Error.WriteLine($"An error occured registering log N. {log.Id}");
        }
    }
}
