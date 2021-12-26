using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Overwurd.Model.Models;

namespace Overwurd.Model.Helpers
{
    public static class RoleHelper
    {
        public static readonly ImmutableDictionary<RoleType, string> RoleTypeToNameMap =
            new Dictionary<RoleType, string>
        {
            [RoleType.Administrator] = "Administrator"
        }.ToImmutableDictionary(x => x.Key, x => x.Value);

        public static string GetRolesString(this Role[] roles) =>
            string.Join(separator: ',', roles.Select(r => r.Name));
    }
}