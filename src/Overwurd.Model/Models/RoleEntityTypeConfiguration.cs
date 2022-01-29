using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Overwurd.Model.Helpers;

namespace Overwurd.Model.Models;

public class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    private static Role CreateRole(RoleType roleType) =>
        new(roleType, RoleHelper.RoleTypeToNameMap[roleType]);

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => (int) x.RoleType);
        builder.Property(x => x.Name);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(Enum.GetValues<RoleType>().Select(CreateRole));
    }
}