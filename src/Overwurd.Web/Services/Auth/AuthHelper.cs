using System.Text;

namespace Overwurd.Web.Services.Auth
{
    public static class AuthHelper
    {
        public static byte[] GetBytesFromSigningKey(string key) =>
            Encoding.ASCII.GetBytes(key);
    }
}