using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AttachmentFiles.Core;
using AttachmentFiles.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace AttachmentFiles.Api.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class FilesController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;

        public FilesController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }


        // GET: api/Files
        [HttpGet("Files/Details")]
        public async Task<ActionResult> Details(string nameFile)
        {
            //http://localhost:59595/api/Files/Details?nameFile=20180419145132199_2pgovn7p.jpg
            try
            {
                var str = nameFile.Split('.');
                if (str.Length == 0)
                    return NotFound(nameFile);
                //20180424
                var folder = nameFile.Substring(0, 4) + "\\" + nameFile.Substring(4, 2) + "\\" + nameFile.Substring(6, 2);

                var filePath = Path.Combine(_appSettings.Value.SaveAllPath, folder, nameFile);
                var image = System.IO.File.OpenRead(filePath);
                if (image == null)
                    return NotFound(nameFile);

                var content = Core.Helpers.CommonHelper.GetContentType(filePath);
                return File(image, content);
            }
            catch (Exception ex) { return NotFound(nameFile); }
        }


        // GET: api/Files
        [HttpGet("Files")]
        public async Task<string> GetSaveFromUrl(string url)
        {
            //http://localhost:59595/api/Files?url=https://scontent-ort2-1.xx.fbcdn.net/v/t1.15752-9/31290658_802278953296732_8367508251603894272_n.jpg?_nc_cat=0&_nc_ad=z-m&_nc_cid=0&oh=1bedb8117ac4f8d51dcea69fd017fa5f&oe=5B543444
            try
            {
                var urlFull = Request.QueryString.Value.ToString();

                url = urlFull.Substring(5, urlFull.Length - 5);

                if (!string.IsNullOrWhiteSpace(url))
                {
                    var sp = url.Split('/');
                    string sp0 = sp[sp.Length - 1];
                    var fileName = "";
                    var sp1 = sp0.Split(new string[] { "=", "?" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var v in sp1)
                    {
                        if (v.IndexOf(".") >= 0)
                        {
                            var sp2 = v.Split(".");

                            if (sp2[sp2.Length - 1].Length <= 4)
                            {
                                fileName = v;
                                break;
                            }
                        }
                    }
                    if (fileName == "")
                    {
                        var name = Request.Query["file_name"].ToString();
                        if (name != null)
                        {
                            fileName = name;
                        }
                    }
                    var now = DateTime.Now;
                    var fileId = now.ToString("yyyyMMddHHmmssfff") + "_" + fileName;
                    var folder = now.ToString("yyyy") + "\\" + now.ToString("MM") + "\\" + now.ToString("dd") + "\\";
                    var dir = _appSettings.Value.SaveAllPath + folder;
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    var fullName = Path.Combine(dir, fileId);

                    using (var client = new HttpClient())
                    using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                    using (Stream contentStream = (client.SendAsync(request)).Result.Content.ReadAsStreamAsync().Result,
                     stream = new FileStream(fullName, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                    {
                        contentStream.CopyTo(stream);
                    }

                    return _appSettings.Value.IBaseUrls.PublichUrl + "api/Files/Details?nameFile=" + fileId;

                }
            }
            catch (Exception ex) { return ex.Message; }
            return url;
        }


        [HttpPost("UploadFile")]
        public async Task<string> UploadFile()
        {
            try
            {
                string fileName = "";
                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        fileName = ContentDispositionHeaderValue
                                  .Parse(file.ContentDisposition)
                                  .FileName.ToString();
                        fileName = Core.Helpers.CommonHelper.removeSpecialFile(fileName);

                        var now = DateTime.Now;
                        var fileId = now.ToString("yyyyMMddHHmmssfff") + "_" + fileName;
                        var folder = now.ToString("yyyy") + "\\" + now.ToString("MM") + "\\" + now.ToString("dd") + "\\";
                        var dir = _appSettings.Value.SaveAllPath + folder;
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        var fullName = Path.Combine(dir, fileId);

                        using (FileStream fs = System.IO.File.Create(fullName))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                        return _appSettings.Value.IBaseUrls.PublichUrl + "api/Files/Details?nameFile=" + fileId;
                    }
                }
            }
            catch (Exception ex) { }
            return "";
        }

    }
}

