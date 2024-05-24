using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Entities
{
    public interface IConfigurationManager
    {
        public Configurations.Settings LoadSettings();
        public void SaveSettings(Configurations.Settings settings);
        event EventHandler ConfigurationChanged;
    }
}
