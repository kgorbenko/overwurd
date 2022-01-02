using System;
using JetBrains.Annotations;

namespace Overwurd.Model.Models
{
    public class User : IEntity
    {
        public long Id { get; [UsedImplicitly] private set; }

        public DateTimeOffset CreatedAt { get; [UsedImplicitly] private set; }

        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string Login { get; set; }

        public string NormalizedLogin { get; set; }

        public string Password { get; set; }

        public Role[] Roles { get; set; } = Array.Empty<Role>();
    }
}