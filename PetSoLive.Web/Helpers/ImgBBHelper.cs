using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;

namespace PetSoLive.Web.Helpers
{
    public class ImgBBHelper
    {
        private readonly string _apiKey;
        public ImgBBHelper(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes)
        {
            using var client = new HttpClient();
            var base64Image = Convert.ToBase64String(imageBytes);
            var content = new MultipartFormDataContent
            {
                { new StringContent(_apiKey), "key" },
                { new StringContent(base64Image), "image" }
            };

            var response = await client.PostAsync("https://api.imgbb.com/1/upload", content);
            var json = await response.Content.ReadAsStringAsync();
            var url = JObject.Parse(json)["data"]?["url"]?.ToString();
            return url;
        }
    }
} 