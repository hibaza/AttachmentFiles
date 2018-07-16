using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace AttachmentFiles.Core.Helpers
{
    public class WebHelper
    {

        public static T ConvertJsonToClass<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T HttpGet<T>(string url)
        {
            using (var client = new HttpClient())
            {
                var json = client.GetStringAsync(url).Result;
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
        public async static Task<T> HttpGetAsync<T>(string url)
        {
            using (var client = new HttpClient())
            {
                var json = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<T>(json);
            }
        }
        public static async Task<string> GetAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    return reader.ReadToEnd();
                }
            }
        }

        public async static Task<T> HttpPostAsync<T>(string url, dynamic value)
        {
            using (var client = new HttpClient())
            {
                var data = JsonConvert.SerializeObject(value).ToString();
                var response = await client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                return await Task.Run(() => JsonConvert.DeserializeObject<T>(content));
            }
        }

        public async static Task<bool> HttpPostAsync(string url, dynamic value)
        {
            using (var client = new HttpClient())
            {
                var data = JsonConvert.SerializeObject(value).ToString();
                var result = client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/json")).Result;
            }
            return true;
        }
       
        
        public async static Task<bool> HttpDeleteAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var result = await client.DeleteAsync(url);
            }
            return true;
        }

        public static dynamic HttpPost(string url, dynamic value)
        {
            HttpResponseMessage result;
            using (var client = new HttpClient())
            {
                var data = JsonConvert.SerializeObject(value).ToString();
                result = client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/json")).Result;
            }
            return result;
        }


        public static dynamic JsonDeserializeDynamic(string url)
        {
            using (var client = new HttpClient())
            {
                var json = client.GetStringAsync(url).Result;
                return JsonConvert.DeserializeObject(json);
            }
        }
        public static string JsonDeserializeString(string url)
        {
            using (var client = new HttpClient())
            {
                var json = client.GetStringAsync(url).Result;
                return json;
            }
        }
    }
}
