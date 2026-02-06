using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace trizbd.Data;

public class CinemaDbContextFactory: IDesignTimeDbContextFactory<CinemaDbContext>
{
    public CinemaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CinemaDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=HOME-PC\\MSSQLSERVER01;Database=CinemaARM;Trusted_Connection=True;TrustServerCertificate=True");

        return new CinemaDbContext(optionsBuilder.Options);
    }
}