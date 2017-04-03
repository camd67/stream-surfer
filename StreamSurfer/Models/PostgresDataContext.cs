﻿using Microsoft.EntityFrameworkCore;

namespace StreamSurfer.Models
{
    public class PostgresDataContext : DbContext
    {
        public PostgresDataContext(DbContextOptions<PostgresDataContext> options)
            : base(options) { }
        public DbSet<Show> Shows { get; set; }
        public DbSet<Synonym> Synonyms { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ShowService> ShowServices { get; set; }
    }
}
