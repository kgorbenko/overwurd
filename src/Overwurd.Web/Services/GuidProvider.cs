using System;

namespace Overwurd.Web.Services
{
    public class GuidProvider : IGuidProvider
    {
        public Guid GenerateGuid()
        {
            return Guid.NewGuid();
        }
    }
}