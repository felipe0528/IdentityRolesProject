using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityRolesProject.Models.ReCaptcha
{
    public interface IReCapchaService
    {
        Task<ReCaptchaResponse> GetResponse(string Token);
    }

    public class ReCapchaService : IReCapchaService
    {
        private ReCaptchaSettings _settings;

        public ReCapchaService(IOptions<ReCaptchaSettings> settings)
        {
            _settings = settings.Value;
        }

        public virtual async Task<ReCaptchaResponse> GetResponse(string Token)
        {
            ReCaptchaData MyData = new ReCaptchaData
            {
                response = Token,
                secret = _settings.SecretKey
            };

            HttpClient client = new HttpClient();
            var response = await client
                .GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={MyData.secret}&response={MyData.response}");
            var googleResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(response);

            return googleResponse;
        }
    }


    public class ReCaptchaData
    {
        public string response { get; set; }
        public string secret { get; set; }
    }

    public class ReCaptchaResponse
    {
        public bool success { get; set; }
        public double score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
