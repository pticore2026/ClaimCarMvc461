using System;
using System.Linq;
using System.Web.Mvc;
using ClaimCar.Web.Models;
using ClaimCar.Web.Services;
namespace ClaimCar.Web.Controllers
{
    [Authorize]
    public class QuoteController : Controller
    {
        private readonly ClaimService _service=new ClaimService();
        public ActionResult Edit(int claimId)
        {
            var claim=_service.Repository.Get(claimId);
            ViewBag.Claim=claim;
            if(claim==null)return HttpNotFound();
            ViewBag.VehiclePolicy=_service.Repository.GetVehiclePolicy(claim.PolicyNumber);
            ViewBag.ClaimId=claimId;
            return View(_service.Repository.GetQuote(claimId));
        }
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Edit(QuoteViewModel m)
        {
            ViewBag.Claim=_service.Repository.Get(m.ClaimId);
            VehiclePolicy policy=null;
            if(ViewBag.Claim!=null)policy=_service.Repository.GetVehiclePolicy(ViewBag.Claim.PolicyNumber);
            ViewBag.VehiclePolicy=policy;
            if(policy!=null)m.DeductibleAmount=policy.Deductible*Math.Max(0,m.DeductibleCases);
            ModelState.Remove("DeductibleAmount");
            ViewBag.ClaimId=m.ClaimId;
            m.Items=m.Items??new System.Collections.Generic.List<QuoteItem>();
            m.RepairTotal=m.Items.Where(x=>string.Equals(x.Proposal,"Sửa chữa",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.PartAmount);
            m.SpecialReplacementTotal=m.Items.Where(x=>string.Equals(x.Proposal,"Thay thế đặc biệt có thu hồi",StringComparison.OrdinalIgnoreCase)||string.Equals(x.Proposal,"Thay thế đặc biệt không thu hồi",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.PartAmount);
            m.ReplacementTotal=m.Items.Where(x=>string.Equals(x.Proposal,"Thay thế có thu hồi",StringComparison.OrdinalIgnoreCase)||string.Equals(x.Proposal,"Thay thế không thu hồi",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.PartAmount);
            m.PaintTotal=m.Items.Sum(x=>x.PaintCost);
            m.LaborTotal=m.Items.Sum(x=>x.LaborCost);

            // Các tổng tiền này được tính lại từ danh sách phụ tùng, không lấy từ dữ liệu post lên.
            ModelState.Remove("ReplacementTotal");
            ModelState.Remove("SpecialReplacementTotal");
            ModelState.Remove("RepairTotal");
            ModelState.Remove("PaintTotal");
            ModelState.Remove("LaborTotal");

            ReplaceFieldError("RepairDiscountPercent", "Vui lòng nhập tỷ lệ giảm giá sửa chữa.");
            ReplaceFieldError("PaintDiscountPercent", "Vui lòng nhập tỷ lệ giảm giá sơn.");

            if(m.NecessaryReasonableCost<0||m.SupplementalDeductibleAmount<0||m.UncoveredDamageValue<0||m.DeductibleAmount<0||m.CompensationReductionAmount<0)
                ModelState.AddModelError("","Các khoản chi phí, thiệt hại và khấu trừ không được âm.");
            if(m.RepairDiscountPercent<0||m.RepairDiscountPercent>100||m.PaintDiscountPercent<0||m.PaintDiscountPercent>100||m.LaborDiscountPercent<0||m.LaborDiscountPercent>100||m.ReplacementDiscountPercent<0||m.ReplacementDiscountPercent>100||m.SpecialReplacementDiscountPercent<0||m.SpecialReplacementDiscountPercent>100||m.TowingDiscountPercent<0||m.TowingDiscountPercent>100)
                ModelState.AddModelError("","Các tỷ lệ giảm giá phải nằm trong khoảng 0% đến 100%.");
            if(m.CompensationReductionPercent<0||m.RiskSharingPercent<0||m.CompensationReductionPercent+m.RiskSharingPercent>100)
                ModelState.AddModelError("","Tổng tỷ lệ giảm trừ bồi thường và chia sẻ rủi ro phải nằm trong khoảng 0% đến 100%.");

            if(policy==null)
                ModelState.AddModelError("","Không xác định được hợp đồng để tính số tiền bồi thường.");
            else if(m.ActualValue<=0)
                ModelState.AddModelError("ActualValue","Giá trị thực tế tại thời điểm cấp đơn phải lớn hơn 0.");
            else
            {
                var replacementAmount=m.ReplacementTotal*(100m-m.ReplacementDiscountPercent)/100m;
                var specialReplacementAmount=m.SpecialReplacementTotal*(100m-m.SpecialReplacementDiscountPercent)/100m;
                var replacementCost=replacementAmount+specialReplacementAmount;
                var depreciation=(replacementAmount*m.ReplacementDepreciationPercent/100m)
                    +(specialReplacementAmount*m.SpecialDepreciationPercent/100m);
                var repairCost=(m.RepairTotal*(100m-m.RepairDiscountPercent)/100m)
                    +(m.PaintTotal*(100m-m.PaintDiscountPercent)/100m)
                    +(m.LaborTotal*(100m-m.LaborDiscountPercent)/100m);
                var towingCost=m.TowingTotal*(100m-m.TowingDiscountPercent)/100m;
                var coveredAmount=(repairCost+replacementCost-depreciation+towingCost)
                    *(policy.InsuredAmount/m.ActualValue);
                var afterDeductible=Math.Max(0,coveredAmount-m.DeductibleAmount);
                if(m.CompensationReductionAmount>0)
                    m.CompensationReductionPercent=afterDeductible>0?Math.Min(100m,m.CompensationReductionAmount*100m/afterDeductible):0m;
                var compensationReduction=afterDeductible*m.CompensationReductionPercent/100m;
                m.CompensationReductionAmount=compensationReduction;
                var riskSharing=afterDeductible*m.RiskSharingPercent/100m;
                m.ApprovedTotal=Math.Max(0,afterDeductible-compensationReduction-riskSharing);
                m.CustomerPaymentTotal=Math.Max(0,coveredAmount-m.ApprovedTotal);
            }
            ModelState.Remove("CustomerPaymentTotal");
            ModelState.Remove("ApprovedTotal");
            ModelState.Remove("CompensationReductionPercent");
            ModelState.Remove("CompensationReductionAmount");
            if(!ModelState.IsValid)return View(m);

            try{var e=_service.SaveQuote(m);if(e!=null){ModelState.AddModelError("",e);return View(m);}TempData["Success"]="Đã tính và lưu báo giá.";return RedirectToAction("Edit",new{claimId=m.ClaimId});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(m);}
        }

        private void ReplaceFieldError(string fieldName, string message)
        {
            ModelState state;
            if(!ModelState.TryGetValue(fieldName, out state)||state.Errors.Count==0)return;
            state.Errors.Clear();
            state.Errors.Add(message);
        }
    }
}
