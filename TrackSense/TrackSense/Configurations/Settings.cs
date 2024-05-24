using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Configurations
{
    public class Settings
    {
        public string ApiUrl { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int ScreenRotation { get; set; } = 0;
        public string Endpoint { get; set; } = null!;
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
