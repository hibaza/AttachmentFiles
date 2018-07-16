using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AttachmentFiles.Core.Helpers
{
   public class ConvertHelper
    {
        public async static Task<Dictionary<string, string>> ConvertParaToDicAsync(string para)
        {
            try
            {
                var t = Task<Dictionary<string, string>>.Factory.StartNew(() =>
                {
                    var dicPara = new Dictionary<string, string>();
                    if (para == null || para == "" || para == "[]")
                        dicPara = null;
                    else
                    {
                        JObject pr = JObject.Parse(para);
                        foreach (var j in pr)
                        {
                            dicPara.Add(j.Key.Trim().ToLower(), j.Value.ToString().Trim());
                        }
                    }
                    return dicPara;
                });
                return await t;
            }
            catch (Exception ex)
            { return null; }
        }


        public async static Task<Dictionary<string, string>> ConvertConfigToDicAsync(string config)
        {
            try
            {
                var t = Task<Dictionary<string, string>>.Factory.StartNew(() =>
                {
                    var dicConfig = new Dictionary<string, string>();

                    if (config == null || config == "" || config == "[]")
                        return null;
                    JObject cf = JObject.Parse(config);
                    foreach (var j in cf)
                    {
                        dicConfig.Add(j.Key.ToLower().Trim(), j.Value.ToString().Trim());
                    }
                    return dicConfig;
                });
                return await t;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async static Task<string> RandomSync(int lengths)
        {
            try
            {
                var t = Task<string>.Factory.StartNew(() => {
                    string all = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    string str = "";
                    Random rnd = new Random();
                    for (int i = 0; i <= lengths; i++)
                    {
                        int iRandom = rnd.Next(5, all.Length - 1);
                        str += all.Substring(iRandom, 1);
                    }
                    return str;
                });
                return await t;
            }
            catch (Exception ex) { return null; }
        }

    }
}
