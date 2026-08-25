using System.Configuration;
namespace ClaimCar.Web.Repositories
{
    public static class ClaimRepositoryFactory
    {
        public static IClaimRepository Create(){var mode=(ConfigurationManager.AppSettings["Data.Mode"]??"Demo").Trim(); return mode.Equals("Oracle",System.StringComparison.OrdinalIgnoreCase)?(IClaimRepository)new OracleClaimRepository():new DemoClaimRepository();}
    }
}
