using System;

namespace Overwurd.Web.Services;

public interface IGuidProvider
{
    Guid GenerateGuid();
}