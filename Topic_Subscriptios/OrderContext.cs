using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topic_Subscriptios
{
    public  class OrderContext : DbContext
    {
        public DbSet<OrderDB> Orders { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /*IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appSettings.json");
            IConfiguration config= builder.Build();*/
            string uriValue = "https://sakeyvault123.vault.azure.net/";
            var azureKeyVault = new SecretClient(new Uri(uriValue), new DefaultAzureCredential());
            var dbString = azureKeyVault.GetSecret("servicedb1");
            optionsBuilder.UseSqlServer(dbString.Value.Value);

            //optionsBuilder.UseSqlServer(@"Data Source=sqlserverservicebus.database.windows.net;Initial Catalog=OrderDB;User ID=service;Password=ser@1234;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False") ;

        }
    }
}