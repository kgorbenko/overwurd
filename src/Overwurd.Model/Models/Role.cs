using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Overwurd.Model.Models
{
    public class Role
    {
        public Role(RoleType roleType, [NotNull] string name)
        {
            if (!Enum.IsDefined(typeof(RoleType), roleType))
                throw new InvalidEnumArgumentException(nameof(roleType), (int) roleType, typeof(RoleType));

            RoleType = roleType;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public RoleType RoleType { get; private set; }

        public string Name { get; private set; }

        private ICollection<User> Users { get; set; }
    }
}