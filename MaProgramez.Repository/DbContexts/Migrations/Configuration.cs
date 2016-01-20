using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using MaProgramez.Repository.DbContexts;
using MaProgramez.Repository.Entities;

namespace MaProgramez.Repository.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "MaProgramez.Repository.DbContext.AppDbContext";
        }

        protected override void Seed(AppDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            if (context.Clients.Any())
            {
                return;
            }

            context.Clients.AddRange(BuildClientsList());
            context.SaveChanges();
        }

        private static IEnumerable<Client> BuildClientsList()
        {

            var clientsList = new List<Client> 
            {
                new Client
                { Id = "maprogramezJSApp", 
                    Secret= Helper.GetHash("jsApp@MaProgramez.net"), 
                    Name="MaProgramez.net JavaScript - NonConfidential app", 
                    ApplicationType =  ApplicationTypes.JavaScript, 
                    Active = true, 
                    RefreshTokenLifeTime = 7200, 
                    AllowedOrigin = "http://localhost:44400"
                },
                 new Client
                { Id = "maprogramezMobileApp", 
                    Secret= Helper.GetHash("mobileApp@MaProgramez.net"), 
                    Name="MaProgramez.net Mobile - NonConfidential app", 
                    ApplicationType =  ApplicationTypes.JavaScript, 
                    Active = true, 
                    RefreshTokenLifeTime = 7200, 
                    AllowedOrigin = "https://maprogramez.net"
                },
                new Client
                { Id = "maprogramezNativeApp", 
                    Secret=Helper.GetHash("nativeApp@MaProgramez.net"), 
                    Name="MaProgramez.net Native - Confidential app", 
                    ApplicationType = ApplicationTypes.NativeConfidential, 
                    Active = true, 
                    RefreshTokenLifeTime = 14400, 
                    AllowedOrigin = "*"
                }
            };

            return clientsList;
        }
    }
}
