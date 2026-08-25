using System.Configuration;
namespace ClaimCar.Web.Infrastructure
{
    public class ConfigLoginService
    {
        public bool Validate(string user,string pass){return user==ConfigurationManager.AppSettings["Login.Username"] && pass==ConfigurationManager.AppSettings["Login.Password"];}
        public string DisplayName { get { return ConfigurationManager.AppSettings["Login.DisplayName"] ?? "Người dùng"; } }
    }
}
