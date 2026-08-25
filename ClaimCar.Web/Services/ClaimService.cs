using System;
using ClaimCar.Sdk;
using ClaimCar.Web.Infrastructure;
using ClaimCar.Web.Models;
using ClaimCar.Web.Repositories;
namespace ClaimCar.Web.Services
{
    public class ClaimService
    {
        private readonly IClaimRepository _repo;
        private readonly ClaimExtensionHost _extensions;
        public ClaimService(){_repo=ClaimRepositoryFactory.Create();_extensions=new ClaimExtensionHost();}
        public IClaimRepository Repository { get { return _repo; } }
        public string ValidateClaim(Claim x,int? exceptId)
        {
            if(x.NotificationDate < x.AccidentDate) return "Ngày thông báo không được trước ngày xảy ra.";
            if(_repo.ClaimNumberExists(x.ClaimNumber,exceptId)) return "Số hồ sơ đã tồn tại.";
            var ctx=Ctx("Thông tin chung",exceptId.HasValue?ExtensionOperation.Update:ExtensionOperation.Create,x);
            var vr=_extensions.Validate(ctx); return vr.IsValid?null:vr.Message;
        }
        public int SaveClaim(Claim x)
        {
            var isNew=x.Id==0; var ctx=Ctx("Thông tin chung",isNew?ExtensionOperation.Create:ExtensionOperation.Update,x); _extensions.BeforeSave(ctx);
            if(isNew)x.Id=_repo.Insert(x); else _repo.Update(x); _extensions.AfterSave(ctx); return x.Id;
        }
        public void DeleteClaim(Claim x){var ctx=Ctx("Thông tin chung",ExtensionOperation.Delete,x);_extensions.BeforeDelete(ctx);_repo.Delete(x.Id);_extensions.AfterDelete(ctx);}
        public string SaveLoss(LossPaymentViewModel x){var ctx=Ctx("Tổn thất - Chi trả",ExtensionOperation.Update,x);var vr=_extensions.Validate(ctx);if(!vr.IsValid)return vr.Message;_extensions.BeforeSave(ctx);_repo.SaveLossPayment(x);_extensions.AfterSave(ctx);return null;}
        public string SaveQuote(QuoteViewModel x){var ctx=Ctx("Báo giá",ExtensionOperation.Update,x);var vr=_extensions.Validate(ctx);if(!vr.IsValid)return vr.Message;_extensions.BeforeSave(ctx);_repo.SaveQuote(x);_extensions.AfterSave(ctx);return null;}
        private static ExtensionContext Ctx(string module,ExtensionOperation op,object entity){return new ExtensionContext{Module=module,Operation=op,Entity=entity,UserName=System.Web.HttpContext.Current==null?"system":System.Web.HttpContext.Current.User.Identity.Name};}
    }
}
