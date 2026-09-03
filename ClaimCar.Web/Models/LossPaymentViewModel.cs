using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ClaimCar.Web.Models
{
    public class CoverageLine
    {
        public int Id { get; set; }
        public string CoverageCode { get; set; }
        public string Currency { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal LossPercent { get; set; }
        public decimal LossAmount { get; set; }
        public decimal Deductible { get; set; }
        public decimal CompensationAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }
    public class BeneficiaryLine { public int Id { get; set; } public string Code { get; set; } public string Name { get; set; } }
    public class ThirdPartyLine { public int Id { get; set; } public string Name { get; set; } public string Currency { get; set; } public decimal Amount { get; set; } }
    public class LossPaymentViewModel
    {
        public int ClaimId { get; set; }
        [Required] public string CauseCode { get; set; }
        public string BehaviorCode { get; set; }
        [Required] public string AreaCode { get; set; }
        [Required] public string EventCode { get; set; }
        public string TbtnYcbReference { get; set; }
        public decimal VehicleCertificateValue { get; set; }
        [Required] public string AccidentDescription { get; set; }
        [Required] public string CauseDescription { get; set; }
        [Required] public string ConsequenceDescription { get; set; }
        public string GarageCode { get; set; }
        public string GarageName { get; set; }
        public string GaragePhone { get; set; }
        public string GarageEmail { get; set; }
        public bool PayThroughGarage { get; set; }
        public bool AssociationFund { get; set; }
        public IList<CoverageLine> Coverages { get; set; }
        public IList<BeneficiaryLine> OtherBeneficiaries { get; set; }
        public IList<ThirdPartyLine> ThirdParties { get; set; }
    }
}
