using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArtClass.Infrastructure.Data;

public sealed class ArtClassDbContextFactory : IDesignTimeDbContextFactory<ArtClassDbContext>
{
    public ArtClassDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArtClassDbContext>();
        optionsBuilder.UseSqlite("Data Source=artclass.db");
        return new ArtClassDbContext(optionsBuilder.Options);
    }
}
