using System;
using JetBrains.Annotations;

namespace Overwurd.Model.Models
{
    public class User : IEntity
    {
        public int Id { get; [UsedImplicitly] private set; }

        public DateTimeOffset CreatedAt { get; [UsedImplicitly] private set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string Password { get; set; }

        public Role[] Roles { get; set; } = Array.Empty<Role>();
    }
}