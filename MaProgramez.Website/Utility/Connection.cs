namespace MaProgramez.Website.Utility
{
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;

    public static class Connection
    {
        private static readonly string EncryptionKey = Common.GetAppConfig("PublicKey");
        private static readonly string ConnStr = @Cryptography.DecryptStringAES(Common.GetAppConfig("ConnectionString"), EncryptionKey);

        public static string ConnectionString;

        static Connection()
        {
             var scsb = new SqlConnectionStringBuilder(ConnStr);
             var ecb = new EntityConnectionStringBuilder
                 {
                     Provider = "System.Data.SqlClient",
                     ProviderConnectionString = scsb.ConnectionString
                 };

            ConnectionString = ecb.ConnectionString;
        }

    }
}
