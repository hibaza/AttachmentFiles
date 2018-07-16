using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AttachmentFiles.Domain
{

    public class AppSettings
    {
        public string Version { get; set; }
        public IBaseUrls IBaseUrls { get; set; }
        public string SavePath { get; set; }
        public string SaveAllPath { get; set; }        
    }

    public class IBaseUrls
    {
        public string PublichUrl { get; set; }
    }
}
