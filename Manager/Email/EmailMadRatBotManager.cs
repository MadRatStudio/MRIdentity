using Infrastructure.System.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Email
{
    public class EmailMadRatBotManager : IEmailService
    {
        public EmailMadRatBotManager(IOptions<EmailConfigurationMadRatBot> settings)
        {
            _emailSettings = (IEmailConfiguration)settings.Value;
        }
    }
}
