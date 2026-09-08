using System;

namespace ClaimCar.Web.Models
{
    public sealed class VehiclePolicy
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        public string CertificateNumber { get; set; }
        public string UnitCode { get; set; }
        public DateTime IssueDate { get; set; }
        public string CustomerCode { get; set; }
        public string OwnerName { get; set; }
        public string CustomerType { get; set; }
        public string IdentityNumber { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string LicensePlate { get; set; }
        public string ChassisNumber { get; set; }
        public string EngineNumber { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int? ManufactureYear { get; set; }
        public string UsagePurpose { get; set; }
        public int? Seats { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public decimal VehicleValue { get; set; }
        public decimal InsuredAmount { get; set; }
        public string CoverageScope { get; set; }
        public string Currency { get; set; }
        public decimal PremiumBeforeTax { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal Deductible { get; set; }
        public string DistributionChannel { get; set; }
        public string IssuedBy { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
    }
}
