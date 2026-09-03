using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
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
            if(!string.IsNullOrWhiteSpace(x.ClaimNumber) && _repo.ClaimNumberExists(x.ClaimNumber.Trim(),exceptId)) return "Số hồ sơ đã tồn tại.";
            if(!exceptId.HasValue)
            {
                var policyError=ValidatePolicy(x);
                if(policyError!=null)return policyError;
            }
            var ctx=Ctx("Thông tin chung",exceptId.HasValue?ExtensionOperation.Update:ExtensionOperation.Create,x);
            var vr=_extensions.Validate(ctx); return vr.IsValid?null:vr.Message;
        }
        public int SaveClaim(Claim x)
        {
            x.ClaimNumber=string.IsNullOrWhiteSpace(x.ClaimNumber)?GenerateClaimNumber():x.ClaimNumber.Trim();
            var isNew=x.Id==0; var ctx=Ctx("Thông tin chung",isNew?ExtensionOperation.Create:ExtensionOperation.Update,x); _extensions.BeforeSave(ctx);
            if(isNew){x.CreatedBy=System.Web.HttpContext.Current==null?"system":System.Web.HttpContext.Current.User.Identity.Name;x.Id=_repo.Insert(x);} else _repo.Update(x); _extensions.AfterSave(ctx); return x.Id;
        }
        public void DeleteClaim(Claim x){var ctx=Ctx("Thông tin chung",ExtensionOperation.Delete,x);_extensions.BeforeDelete(ctx);_repo.Delete(x.Id);_extensions.AfterDelete(ctx);}
        public ClaimDeleteResult DeleteClaims(System.Collections.Generic.IEnumerable<int> ids,string userName,bool continueValid)
        {
            var selected=(ids??new int[0]).Distinct().ToList();
            if(selected.Count==0)return ClaimDeleteResult.Error("Vui lòng chọn ít nhất một hồ sơ bồi thường để xoá.");
            var valid=new System.Collections.Generic.List<Claim>();var invalidStatus=0;var invalidOwner=0;var invalidPermission=0;var missing=0;
            foreach(var id in selected)
            {
                var claim=_repo.Get(id);if(claim==null){missing++;continue;}
                string reason;if(CanDelete(claim,userName,out reason))valid.Add(claim);
                else if(reason=="status")invalidStatus++;else if(reason=="owner")invalidOwner++;else invalidPermission++;
            }
            var invalidCount=selected.Count-valid.Count;
            if(valid.Count==0)return ClaimDeleteResult.Error("Không có hồ sơ hợp lệ để xoá. Hồ sơ phải ở trạng thái Mới tiếp nhận, do bạn tạo và tài khoản phải có quyền xoá.");
            if(invalidCount>0&&!continueValid)
            {
                var reasons=new System.Collections.Generic.List<string>();
                if(invalidStatus>0)reasons.Add(invalidStatus+" hồ sơ không ở trạng thái Mới tiếp nhận");
                if(invalidOwner>0)reasons.Add(invalidOwner+" hồ sơ không do bạn tạo");
                if(invalidPermission>0)reasons.Add(invalidPermission+" hồ sơ bạn không có quyền xoá");
                if(missing>0)reasons.Add(missing+" hồ sơ không còn tồn tại");
                return ClaimDeleteResult.Confirm(valid.Count,invalidCount,string.Join("; ",reasons)+". Bạn có muốn tiếp tục xoá các hồ sơ hợp lệ không?");
            }
            foreach(var claim in valid)DeleteClaim(claim);
            return ClaimDeleteResult.Success(valid.Select(x=>x.Id).ToArray(),invalidCount>0);
        }

        private bool CanDelete(Claim claim,string userName,out string reason)
        {
            if(!string.Equals(claim.Status,"Mới tiếp nhận",StringComparison.OrdinalIgnoreCase)){reason="status";return false;}
            var allowed=(ConfigurationManager.AppSettings["Claim.DeleteUsers"]??"").Split(',').Select(x=>x.Trim());
            if(!allowed.Any(x=>string.Equals(x,userName,StringComparison.OrdinalIgnoreCase))){reason="permission";return false;}
            if(!string.Equals(claim.CreatedBy,userName,StringComparison.OrdinalIgnoreCase)){reason="owner";return false;}
            reason=null;return true;
        }
        public string SaveLoss(LossPaymentViewModel x){var claim=_repo.Get(x.ClaimId);if(claim==null)return "Không tìm thấy hồ sơ bồi thường.";var policyError=ValidatePolicy(claim);if(policyError!=null)return policyError;var amountError=ValidateLossAmounts(x,claim);if(amountError!=null)return amountError;var ctx=Ctx("Tổn thất - Chi trả",ExtensionOperation.Update,x);var vr=_extensions.Validate(ctx);if(!vr.IsValid)return vr.Message;_extensions.BeforeSave(ctx);_repo.SaveLossPayment(x);_extensions.AfterSave(ctx);return null;}
        public string SaveQuote(QuoteViewModel x){var claim=_repo.Get(x.ClaimId);if(claim==null)return "Không tìm thấy hồ sơ bồi thường.";var policyError=ValidatePolicy(claim);if(policyError!=null)return policyError;var amountError=ValidateApprovedAmount(x,claim);if(amountError!=null)return amountError;var ctx=Ctx("Báo giá",ExtensionOperation.Update,x);var vr=_extensions.Validate(ctx);if(!vr.IsValid)return vr.Message;_extensions.BeforeSave(ctx);_repo.SaveQuote(x);_extensions.AfterSave(ctx);return null;}
        private static ExtensionContext Ctx(string module,ExtensionOperation op,object entity){return new ExtensionContext{Module=module,Operation=op,Entity=entity,UserName=System.Web.HttpContext.Current==null?"system":System.Web.HttpContext.Current.User.Identity.Name};}
        private string ValidatePolicy(Claim claim)
        {
            var policy=_repo.GetVehiclePolicy(claim.PolicyNumber);
            if(policy==null)return "Không tìm thấy hợp đồng bảo hiểm '"+claim.PolicyNumber+"'.";
            if(!SameVehicle(policy.LicensePlate,claim.LicensePlate))return "Biển số xe không thuộc hợp đồng bảo hiểm đã chọn.";
            if(!string.Equals(policy.Status,"DA_CAP",StringComparison.OrdinalIgnoreCase))return "Hợp đồng bảo hiểm không khả dụng để yêu cầu bồi thường (trạng thái hiện tại: "+policy.Status+").";
            if(claim.AccidentDate.Date<policy.EffectiveFrom.Date||claim.AccidentDate.Date>policy.EffectiveTo.Date)return "Ngày xảy ra tổn thất nằm ngoài thời hạn hiệu lực hợp đồng bảo hiểm.";
            if(policy.InsuredAmount>0&&claim.InsuredValue!=policy.InsuredAmount)return "Giá trị bảo hiểm trên hồ sơ không khớp số tiền bảo hiểm của hợp đồng.";
            return null;
        }

        private string ValidateLossAmounts(LossPaymentViewModel model,Claim claim)
        {
            var rows=model.Coverages??new System.Collections.Generic.List<CoverageLine>();
            if(rows.Any(x=>x.LossPercent<0||x.LossPercent>100))return "%TT phải nằm trong khoảng từ 0 đến 100.";
            if(rows.Any(x=>x.LossAmount<0||x.Deductible<0||x.CompensationAmount<0||x.TaxAmount<0))return "Giá trị tổn thất, khấu trừ, bồi thường và thuế không được âm.";
            if(rows.Any(x=>x.CompensationAmount>x.LossAmount))return "Tiền bồi thường không được vượt tiền tổn thất trên cùng phạm vi bảo hiểm.";
            var totalLoss=rows.Sum(x=>x.LossAmount);
            var totalCompensation=rows.Sum(x=>x.CompensationAmount);
            var policy=_repo.GetVehiclePolicy(claim.PolicyNumber);
            if(policy==null)return "Không tìm thấy hợp đồng bảo hiểm để kiểm tra hạn mức.";
            var ratio=MaxLossRatio();
            if(policy.VehicleValue<=0)return "Hợp đồng chưa có giá trị xe hợp lệ để kiểm tra hạn mức bồi thường.";
            if(totalLoss>policy.VehicleValue*ratio)return "Tổng tổn thất vượt hạn mức "+ratio.ToString("0.##",CultureInfo.InvariantCulture)+" lần giá trị xe theo hợp đồng.";
            if(policy.InsuredAmount>0&&totalCompensation>policy.InsuredAmount)return "Tổng tiền bồi thường vượt số tiền bảo hiểm của hợp đồng.";
            return null;
        }

        private string ValidateApprovedAmount(QuoteViewModel model,Claim claim)
        {
            if(model.ApprovedTotal<0||model.CustomerPaymentTotal<0)return "Tổng duyệt giá và số tiền khách hàng thanh toán không được âm.";
            var policy=_repo.GetVehiclePolicy(claim.PolicyNumber);
            if(policy==null)return "Không tìm thấy hợp đồng bảo hiểm để kiểm tra hạn mức.";
            if(policy.VehicleValue<=0)return "Hợp đồng chưa có giá trị xe hợp lệ để kiểm tra hạn mức bồi thường.";
            if(model.ApprovedTotal>policy.VehicleValue*MaxLossRatio())return "Tổng duyệt giá vượt hạn mức theo giá trị xe của hợp đồng.";
            if(policy.InsuredAmount>0&&model.ApprovedTotal>policy.InsuredAmount)return "Tổng duyệt giá vượt số tiền bảo hiểm của hợp đồng.";
            return null;
        }

        private static decimal MaxLossRatio()
        {
            decimal value;
            var raw=ConfigurationManager.AppSettings["Claim.MaxLossToVehicleValueRatio"];
            return decimal.TryParse(raw,NumberStyles.Number,CultureInfo.InvariantCulture,out value)&&value>0?value:1m;
        }

        private static bool SameVehicle(string left,string right)
        {
            return string.Equals(NormalizePlate(left),NormalizePlate(right),StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePlate(string value)
        {
            return new string((value??"").Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
        }

        private string GenerateClaimNumber()
        {
            string number;
            do { number="HS-"+DateTime.Now.ToString("yyyyMMdd")+"-"+Guid.NewGuid().ToString("N").Substring(0,8).ToUpperInvariant(); }
            while(_repo.ClaimNumberExists(number,null));
            return number;
        }
    }

public sealed class ClaimDeleteResult
    {
        public string Status { get; private set; }
        public string Message { get; private set; }
        public int ValidCount { get; private set; }
        public int InvalidCount { get; private set; }
        public int[] DeletedIds { get; private set; }
        public bool Partial { get; private set; }
        public static ClaimDeleteResult Error(string message){return new ClaimDeleteResult{Status="error",Message=message,DeletedIds=new int[0]};}
        public static ClaimDeleteResult Confirm(int valid,int invalid,string message){return new ClaimDeleteResult{Status="confirm",Message=message,ValidCount=valid,InvalidCount=invalid,DeletedIds=new int[0]};}
        public static ClaimDeleteResult Success(int[] ids,bool partial){return new ClaimDeleteResult{Status="success",Message=partial?"Đã xoá các hồ sơ hợp lệ; các hồ sơ không hợp lệ được giữ nguyên.":"Đã xoá thành công các hồ sơ đã chọn.",DeletedIds=ids,ValidCount=ids.Length,Partial=partial};}
    }
}
