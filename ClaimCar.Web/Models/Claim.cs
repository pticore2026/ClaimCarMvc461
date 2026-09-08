using System;
using System.ComponentModel.DataAnnotations;

namespace ClaimCar.Web.Models
{
    public class Claim
    {
        public int Id { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), StringLength(10), Display(Name="Mã đơn vị quản lý")]
        public string ManagementUnitCode { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), StringLength(100), Display(Name="Đơn vị quản lý")]
        public string ManagementUnitName { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), StringLength(20), Display(Name="Mã khu vực quản lý")]
        public string ManagementAreaCode { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), StringLength(100), Display(Name="Khu vực quản lý")]
        public string ManagementAreaName { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), StringLength(20), Display(Name="Biển số")]
        public string LicensePlate { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), DataType(DataType.Date), Display(Name="Ngày nhập")]
        public DateTime EntryDate { get; set; }
        [DataType(DataType.Date), Display(Name="Ngày nhập Call")]
        public DateTime? CallEntryDate { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), StringLength(50), Display(Name="Số hợp đồng")]
        public string PolicyNumber { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), StringLength(30), Display(Name="Tình trạng hồ sơ")]
        public string Status { get; set; }
        [DataType(DataType.Date), Display(Name="Ngày quyết định")]
        public DateTime? DecisionDate { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), DataType(DataType.Date), Display(Name="Ngày xảy ra")]
        public DateTime AccidentDate { get; set; }
        [Required(ErrorMessage="{0} bắt buộc nhập"), DataType(DataType.Date), Display(Name="Ngày thông báo")]
        public DateTime NotificationDate { get; set; }
        [StringLength(50), Display(Name="Số hồ sơ")]
        public string ClaimNumber { get; set; }
        [StringLength(30), Display(Name="Mã giám định viên")]
        public string SurveyorCode { get; set; }
        public decimal InsuredValue { get; set; }
        public string CreatedBy { get; set; }
    }
}
