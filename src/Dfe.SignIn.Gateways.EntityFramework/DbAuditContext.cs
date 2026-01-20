using Dfe.SignIn.Core.Entities.Audit;
using Dfe.SignIn.Gateways.EntityFramework.Configuration.Audit;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

#pragma warning disable CS1591
public partial class DbAuditContext : DbContext
{
    public DbAuditContext(DbContextOptions<DbAuditContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLogEntity> AuditLogs { get; set; }

    public virtual DbSet<AuditLogMetaEntity> AuditLogMeta { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AuditLogEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogMetaEntityConfiguration());
    }
}
#pragma warning restore CS1591

