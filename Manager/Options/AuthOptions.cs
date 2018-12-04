using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Manager.Options
{
    public class AuthOptions
    {
        public const string ISSUER = "MadRatStudio";
        public const string AUDIENCE = "http://identity.madrat.studio";
        public const string KEY = "SOME_SECRET_1111-Key!@WW";
        public const int LIFETIME = 604800;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }

    public static class TokenOptions
    {
        public const string USER_ID = "user_id";
    }
}
