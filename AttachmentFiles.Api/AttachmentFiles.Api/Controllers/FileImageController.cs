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
    public class FileImageController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;

        public FileImageController(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }


        // GET: api/File_DocumentsView
        [HttpGet("File_DocumentsView/Details")]
        public async Task<ActionResult> Details(string nameFile)
        {
            //http://localhost:59595/api/File_DocumentsView/Details?nameFile=20180419145132199_2pgovn7p.jpg
            try
            {
                var str = nameFile.Split('.');
                if (str.Length == 0)
                    return NotFound(nameFile);
                //20180424
                var folder = nameFile.Substring(0, 4)+"\\"+ nameFile.Substring(4, 2) + "\\" + nameFile.Substring(6, 2);
                
                var filePath = Path.Combine(_appSettings.Value.SavePath, folder, nameFile);
                var image =  System.IO.File.OpenRead(filePath);
                if (image == null)
                    return NotFound(nameFile);                
                
                return File(image, "image/"+ str[str.Length-1]);
            }
            catch(Exception ex) { return NotFound(nameFile); }
        }

        
        // GET: api/File_SaveFromUrl
        [HttpGet("File_SaveFromUrl")]
        public async Task<string> GetSaveFromUrl( string url)
        {
            //http://localhost:59595/api/File_SaveFromUrl?url=https://scontent-ort2-1.xx.fbcdn.net/v/t1.15752-9/31290658_802278953296732_8367508251603894272_n.jpg?_nc_cat=0&_nc_ad=z-m&_nc_cid=0&oh=1bedb8117ac4f8d51dcea69fd017fa5f&oe=5B543444
            try
            {
                var urlFull = Request.QueryString.Value.ToString();
                
                url = urlFull.Substring(5,urlFull.Length-5);
                                 
                if (!string.IsNullOrWhiteSpace(url))
                {
                    if (url.IndexOf(_appSettings.Value.IBaseUrls.PublichUrl) >= 0)
                        return url;

                    var sp = url.Split('/');
                    string sp0 = sp[sp.Length - 1];
                    var fileName = "";
                    var sp1 = sp0.Split(new string[] { "=", "?" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var v in sp1)
                    {
                        if (v.IndexOf(".") >= 0)
                        {
                            fileName = v;
                            break;
                        }
                    }
                    var now = DateTime.Now;
                    var fileId = now.ToString("yyyyMMddHHmmssfff") + "_" + fileName;
                    var folder = now.ToString("yyyy") + "\\" + now.ToString("MM") + "\\" + now.ToString("dd") + "\\";
                    var dir = _appSettings.Value.SavePath + folder;
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);


                    using (HttpClient client = new HttpClient())
                    {
                        // const string url = "https://github.com/tugberkugurlu/ASPNETWebAPISamples/archive/master.zip";
                        using (
                            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                        {

                            if (fileId.IndexOf(".") < 0)
                            {
                                var type = response.Content.Headers.ContentType.ToString().ToLower();
                                if (type.IndexOf("jpg") >= 0 || type.IndexOf("jpeg") >= 0)
                                    fileId += ".jpg";
                                if (type.IndexOf("tif") >= 0)
                                    fileId += ".tif";
                                if (type.IndexOf("bmp") >= 0)
                                    fileId += ".bmp";
                                if (type.IndexOf("png") >= 0)
                                    fileId += ".png";
                                if (type.IndexOf("eps") >= 0)
                                    fileId += ".eps";
                                if (type.IndexOf("raw") >= 0)
                                    fileId += ".raw";
                                if (type.IndexOf("cr2") >= 0)
                                    fileId += ".cr2";
                            }

                            var fullName = Path.Combine(dir, fileId);

                            using (Stream streamToWriteTo = System.IO.File.Open(fullName, FileMode.Create))
                            {
                                await streamToReadFrom.CopyToAsync(streamToWriteTo);
                            }
                        }
                    }

                   
                        //.tif, .tiff .bmp .jpg, .jpeg .gif .png .eps .raw, .cr2, .nef, .orf, .sr2
                        if (fileId.IndexOf(".tif") >= 0 ||
                            fileId.IndexOf(".tiff") >= 0 ||
                            fileId.IndexOf(".bmp") >= 0 ||
                            fileId.IndexOf(".jpg") >= 0 ||
                            fileId.IndexOf(".jpeg") >= 0 ||
                            fileId.IndexOf(".png") >= 0 ||
                            fileId.IndexOf(".eps") >= 0 ||
                            fileId.IndexOf(".raw") >= 0 ||
                            fileId.IndexOf(".cr2") >= 0
                            )
                            return _appSettings.Value.IBaseUrls.PublichUrl + "api/File_DocumentsView/Details?nameFile=" + fileId;
                        else
                            return url;
                    }
            }
            catch (Exception ex) { }
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
                        var dir = _appSettings.Value.SavePath + folder;
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        var fullName = Path.Combine(dir, fileId);

                        using (FileStream fs = System.IO.File.Create(fullName))
                        {
                            file.CopyTo(fs);
                            fs.Flush();
                        }
                        return _appSettings.Value.IBaseUrls.PublichUrl + "api/File_DocumentsView/Details?nameFile=" + fileId;
                    }
                }
            }
            catch (Exception ex) { }
            return "";
        }

        }
}

