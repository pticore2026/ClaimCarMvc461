using System.Configuration;
namespace ClaimCar.Web.Repositories
{
    public static class ClaimRepositoryFactory
    {
        public static IClaimRepository Create()
        {
            var mode = (ConfigurationManager.AppSettings["Data.Mode"] ?? "Demo").Trim();
            if (mode.Equals("Oracle", System.StringComparison.OrdinalIgnoreCase)) return new OracleClaimRepository();
            if (mode.Equals("MySql", System.StringComparison.OrdinalIgnoreCase)) return new MySqlClaimRepository();
            if (mode.Equals("SQLite", System.StringComparison.OrdinalIgnoreCase)) return new SQLiteClaimRepository();
            return new DemoClaimRepository();
        }
    }
}
