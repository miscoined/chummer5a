using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChummerHub.Models.V1;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using ChummerHub.API;

namespace ChummerHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public IHostingEnvironment HostingEnvironment { get; set; }
        public IConfiguration Configuration { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHostingEnvironment env)
            : base(options)
        {
            HostingEnvironment = env;
        }

        public ApplicationDbContext()
        {
          
        }

        public override int SaveChanges()
        {
            var entities = from e in ChangeTracker.Entries()
                           where e.State == EntityState.Added
                               || e.State == EntityState.Modified
                           select e.Entity;
            bool error = false;
            Collection<ValidationResult> validationResults = new Collection<ValidationResult>();
            foreach (var entity in entities)
            {
                var validationContext = new ValidationContext(entity);
                //Validator.ValidateObject(entity, validationContext);
                if (Validator.TryValidateObject(entity, validationContext, validationResults, true))
                {
                    error = true;
                }
            }
            if (error == true)
            {
                int counter = 0;
                string wholeMessage = "Error while validating Entities:" + Environment.NewLine;
                foreach (var valResult in validationResults)
                {
                    counter++;
                    string msg = "Members " + valResult.MemberNames.Aggregate((a,b) => a + ", " + b) + " not valid: ";
                    msg += valResult.ErrorMessage;
                    wholeMessage += msg + Environment.NewLine;
                }
                var ex = new HubException(wholeMessage);
                throw ex;
            }

            return base.SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            if(!optionsBuilder.IsConfigured)
            {
                
                if (HostingEnvironment == null)
                {
                    string constring = "Server=(localdb)\\mssqllocaldb;Database=SINners_DB;Trusted_Connection=True;MultipleActiveResultSets=true";
                    optionsBuilder.UseSqlServer(constring);
                    //throw new ArgumentNullException("HostingEnviroment is null!");
                }
                else
                {
                    var configurationBuilder = new ConfigurationBuilder()
                        .SetBasePath(HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    Configuration = configurationBuilder.Build();
                    optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                }
            }
            
            optionsBuilder.EnableSensitiveDataLogging();
        }

        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => new { b.TagName, b.TagValue });
            builder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => b.SINnerId);
            builder.Entity<ChummerHub.Models.V1.SINerUserRight>()
                .HasIndex(b => b.SINnerId);
            builder.Entity<ChummerHub.Models.V1.SINner>()
                .HasIndex(b => b.Alias);
            builder.Entity<ChummerHub.Models.V1.SINnerGroup>()
                .HasIndex(b => b.Groupname);
            builder.Entity<ChummerHub.Models.V1.SINerUserRight>()
                .HasIndex(b => b.EMail);
            builder.Entity<ChummerHub.Models.V1.SINnerGroup>()
                .HasIndex(b => b.Language);
            builder.Entity<ChummerHub.Models.V1.Tag>()
                .HasIndex(b => b.TagValueDouble);
        }

        public DbSet<ChummerHub.Models.V1.SINner> SINners { get; set; }

        public DbSet<ChummerHub.Models.V1.SINnerGroup> SINnerGroups { get; set; }

        public DbSet<ChummerHub.Models.V1.Tag> Tags { get; set; }

        public DbSet<ChummerHub.Models.V1.SINerUserRight> UserRights { get; set; }

        public DbSet<ChummerHub.Models.V1.UploadClient> UploadClients { get; set; }

        public DbSet<ChummerHub.Models.V1.SINnerComment> SINnerComments { get; set; }
        public DbSet<ChummerHub.Models.V1.SINnerVisibility> SINnerVisibility { get; set; }
        public DbSet<ChummerHub.Models.V1.SINnerMetaData> SINnerMetaData { get; set; }

        public DbSet<ChummerHub.Models.V1.SINnerGroupSetting> SINnerGroupSettings { get; set; }
    }
}
