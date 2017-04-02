using Microsoft.EntityFrameworkCore;

namespace StreamSurfer.Models
{
    public class PostgresDataContext : DbContext
    {
        public PostgresDataContext(DbContextOptions<PostgresDataContext> options)
            : base(options) { }

    }
}
