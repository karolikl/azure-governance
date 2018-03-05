using SelfServiceWeb.Domain;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SelfServiceWeb.Provisioning
{
    public class Workflow : IWorkflow
    {
        private readonly ISettings _settings;
        public Workflow(ISettings settings)
        {
            _settings = settings;
        }

        public async Task<bool> Trigger(ProvisioningRequest request)
        {
            string url = _settings.GetProvisioningUrl();
            return await CreateResource(request, url);
        }

        async Task<bool> CreateResource(ProvisioningRequest request, string url)
        {
            var client = new HttpClient
            {
                // Update port # in the following line.
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json.ToLower(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.IsSuccessStatusCode;
        }
    }
}
