using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Overwurd.Web.Helpers;

public static class EnvironmentHelper
{
    public static Version GetApplicationVersion() =>
        Assembly.GetExecutingAssembly().GetName().Version;

    public static async Task<string> GetLastDatabaseMigrationAsync(ApplicationDbContext dbContext) =>
        await dbContext.Database.CanConnectAsync()
            ? (await dbContext.Database.GetAppliedMigrationsAsync()).Last()
            : null;
}