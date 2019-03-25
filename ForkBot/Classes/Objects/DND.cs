using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ForkBot
{
    class DND
    {
        private static readonly HttpClient client = new HttpClient();

        public DND()
        {
            GetData().Wait();
        }

        private static async Task GetData()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(""));
                
        }
    }
}
