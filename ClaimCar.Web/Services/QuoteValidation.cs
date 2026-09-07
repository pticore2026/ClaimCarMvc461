using System;
using System.Linq;
using System.Web.Mvc;
using ClaimCar.Web.Models;
namespace ClaimCar.Web.Services
{
    public static class QuoteValidation
    {
        public static void Validate(QuoteViewModel m, VehiclePolicy policy, ModelStateDictionary state, string prefix="")
        {
            if(policy!=null&&policy.VehicleValue>0)
                m.ParticipationValuePercent=Math.Round(policy.InsuredAmount*100m/policy.VehicleValue,2,MidpointRounding.AwayFromZero);
            state.Remove(prefix+"ParticipationValuePercent");
            m.Items=m.Items??new System.Collections.Generic.List<QuoteItem>();
            m.RepairTotal=m.Items.Where(x=>string.Equals(x.Proposal,"Sửa chữa",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.PartAmount);
            m.SpecialReplacementTotal=m.Items.Where(x=>string.Equals(x.Proposal,"Thay thế đặc biệt có thu hồi",StringComparison.OrdinalIgnoreCase)||string.Equals(x.Proposal,"Thay thế đặc biệt không thu hồi",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.PartAmount);
            m.ReplacementTotal=m.Items.Where(x=>string.Equals(x.Proposal,"Thay thế có thu hồi",StringComparison.OrdinalIgnoreCase)||string.Equals(x.Proposal,"Thay thế không thu hồi",StringComparison.OrdinalIgnoreCase)).Sum(x=>x.PartAmount);
            m.PaintTotal=m.Items.Sum(x=>x.PaintCost);
            m.LaborTotal=m.Items.Sum(x=>x.LaborCost);

            // Các tổng tiền này được tính lại từ danh sách phụ tùng, không lấy từ dữ liệu post lên.
            state.Remove(prefix+"ReplacementTotal");
            state.Remove(prefix+"SpecialReplacementTotal");
            state.Remove(prefix+"RepairTotal");
            state.Remove(prefix+"PaintTotal");
            state.Remove(prefix+"LaborTotal");

            ReplaceFieldError(state, prefix+"RepairDiscountPercent", "Vui lòng nhập tỷ lệ giảm giá sửa chữa.");
            ReplaceFieldError(state, prefix+"PaintDiscountPercent", "Vui lòng nhập tỷ lệ giảm giá sơn.");

            if(m.NecessaryReasonableCost<0||m.SupplementalDeductibleAmount<0||m.UncoveredDamageValue<0||m.DeductibleAmount<0||m.CompensationReductionAmount<0)
                state.AddModelError(prefix+"","Các khoản chi phí, thiệt hại và khấu trừ không được âm.");
            if(m.RepairDiscountPercent<0||m.RepairDiscountPercent>100||m.PaintDiscountPercent<0||m.PaintDiscountPercent>100||m.LaborDiscountPercent<0||m.LaborDiscountPercent>100||m.ReplacementDiscountPercent<0||m.ReplacementDiscountPercent>100||m.SpecialReplacementDiscountPercent<0||m.SpecialReplacementDiscountPercent>100||m.TowingDiscountPercent<0||m.TowingDiscountPercent>100)
                state.AddModelError(prefix+"","Các tỷ lệ giảm giá phải nằm trong khoảng 0% đến 100%.");
            if(m.CompensationReductionPercent<0||m.RiskSharingPercent<0||m.CompensationReductionPercent+m.RiskSharingPercent>100)
                state.AddModelError(prefix+"","Tổng tỷ lệ giảm trừ bồi thường và chia sẻ rủi ro phải nằm trong khoảng 0% đến 100%.");
            if(m.ParticipationValuePercent<0||m.ParticipationValuePercent>100||m.ParticipationFeePercent<0||m.ParticipationFeePercent>100)
                state.AddModelError(prefix+"","Tỷ lệ giá trị và tỷ lệ phí tham gia bảo hiểm phải nằm trong khoảng 0% đến 100%.");
            if(m.ReplacementDepreciationPercent<0||m.ReplacementDepreciationPercent>100||m.SpecialDepreciationPercent<0||m.SpecialDepreciationPercent>100)
                state.AddModelError(prefix+"","Các tỷ lệ khấu hao phải nằm trong khoảng 0% đến 100%.");

            if(policy==null)
                state.AddModelError(prefix+"","Không xác định được hợp đồng để tính số tiền bồi thường.");
            else if(m.ActualValue<=0)
                state.AddModelError(prefix+"ActualValue","Giá trị thực tế tại thời điểm cấp đơn phải lớn hơn 0.");
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
                var participationValueAmount=afterDeductible*(100m-m.ParticipationValuePercent)/100m;
                var participationFeeAmount=afterDeductible*(100m-m.ParticipationFeePercent)/100m;
                m.CustomerPaymentTotal=Math.Max(0,depreciation+participationValueAmount+participationFeeAmount+m.DeductibleAmount+compensationReduction+riskSharing);
            }
            state.Remove(prefix+"CustomerPaymentTotal");
            state.Remove(prefix+"ApprovedTotal");
            state.Remove(prefix+"CompensationReductionPercent");
            state.Remove(prefix+"CompensationReductionAmount");
            if(string.IsNullOrWhiteSpace(m.ApprovalType))state.AddModelError(prefix+"ApprovalType","Vui lòng chọn kiểu duyệt.");
        }
        private static void ReplaceFieldError(ModelStateDictionary state,string fieldName,string message)
        {
            ModelState field;
            if(!state.TryGetValue(fieldName,out field)||field.Errors.Count==0)return;
            field.Errors.Clear();field.Errors.Add(message);
        }
    }
}
