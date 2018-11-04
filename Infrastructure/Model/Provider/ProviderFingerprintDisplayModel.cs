using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Model.Provider
{
    public class ProviderFingerprintDisplayModel
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Fingerprint { get; set; }
        public DateTime FingerprintUpdateTime { get; set; }
    }
}
