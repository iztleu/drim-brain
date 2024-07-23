using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace BlockchainService.Database;

public class BlockchainDbContext(DbContextOptions<BlockchainDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddQuartz(builder =>
        {
            builder.UsePostgreSql(schema: "public");
        });
    }
}
