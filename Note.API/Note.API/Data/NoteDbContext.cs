using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notes.API.Models.Entities;

namespace Notes.API.Data
{
    public class NoteDbContext : IdentityDbContext<AppUser>
    {
        public NoteDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Note> Note { get; set; }
    }
}
