using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.System.Appsettings
{
    public class ExternalServiceSettings
    {
        public ExternalServiceGoogleSettnigs Google { get; set; }
    }

    public class ExternalServiceGoogleSettnigs
    {
        public string Key { get; set; }
    }
}
