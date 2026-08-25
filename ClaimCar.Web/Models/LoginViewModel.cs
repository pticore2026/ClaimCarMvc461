using System.ComponentModel.DataAnnotations;
namespace ClaimCar.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage="Vui lòng nhập tài khoản")]
        [Display(Name="Tài khoản")]
        public string UserName { get; set; }
        [Required(ErrorMessage="Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name="Mật khẩu")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
