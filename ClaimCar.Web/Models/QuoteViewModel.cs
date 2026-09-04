using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ClaimCar.Web.Models
{
    public class QuoteItem
    {
        public int Id { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public string Damage { get; set; }
        public string Dimensions { get; set; }
        public string Proposal { get; set; }
        public string PartType { get; set; }
        public decimal PartPrice { get; set; }
        public decimal PaintCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartAmount { get { return Quantity * PartPrice; } }
        public decimal Total { get { return PartAmount + PaintCost + LaborCost; } }
    }
    public class QuoteViewModel
    {
        public int ClaimId { get; set; }
        public string ApprovalType { get; set; }
        public string CompensationMethod { get; set; }
        public decimal ActualValue { get; set; }
        public decimal UncoveredDamageValue { get; set; }
        public DateTime? SubmitDate { get; set; }
        public string ReductionReason { get; set; }
        public IList<QuoteItem> Items { get; set; }
        public decimal ReplacementTotal { get; set; }
        public decimal SpecialReplacementTotal { get; set; }
        public decimal RepairTotal { get; set; }
        public decimal PaintTotal { get; set; }
        public decimal LaborTotal { get; set; }
        public decimal TowingTotal { get; set; }
        public decimal NecessaryReasonableCost { get; set; }
        [Required(ErrorMessage="Vui lòng nhập tỷ lệ giảm giá sửa chữa.")]
        [Display(Name="Tỷ lệ giảm giá sửa chữa")]
        public decimal RepairDiscountPercent { get; set; }

        [Required(ErrorMessage="Vui lòng nhập tỷ lệ giảm giá sơn.")]
        [Display(Name="Tỷ lệ giảm giá sơn")]
        public decimal PaintDiscountPercent { get; set; }
        public decimal LaborDiscountPercent { get; set; }

        [Required(ErrorMessage="Vui lòng nhập tỷ lệ giảm giá thay thế.")]
        [Display(Name="Tỷ lệ giảm giá thay thế")]
        public decimal ReplacementDiscountPercent { get; set; }
        public decimal SpecialReplacementDiscountPercent { get; set; }
        public decimal TowingDiscountPercent { get; set; }
        public decimal ReplacementDepreciationPercent { get; set; }
        public decimal SpecialDepreciationPercent { get; set; }
        public decimal ParticipationValuePercent { get; set; }
        public decimal ParticipationFeePercent { get; set; }
        public int DeductibleCases { get; set; }
        public decimal DeductibleAmount { get; set; }
        public decimal SupplementalDeductibleAmount { get; set; }
        public decimal CompensationReductionPercent { get; set; }
        public decimal CompensationReductionAmount { get; set; }
        public decimal RiskSharingPercent { get; set; }
        public decimal CustomerPaymentTotal { get; set; }
        public decimal ApprovedTotal { get; set; }
        public string Checker { get; set; }
    }
}
