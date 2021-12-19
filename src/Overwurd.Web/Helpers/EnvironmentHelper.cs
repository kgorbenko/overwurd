using System;
using System.Reflection;

namespace Overwurd.Web.Helpers
{
    public static class EnvironmentHelper
    {
        public static Version GetApplicationVersion()
            => Assembly.GetExecutingAssembly().GetName().Version;
    }
}