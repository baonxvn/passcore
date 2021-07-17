namespace Unosquare.PassCore.Web.Models
{
    using System.ComponentModel.DataAnnotations;
    using Unosquare.PassCore.Common;

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = nameof(ApiErrorCode.FieldRequired))]
        public string Username { get; set; }

        [Required(ErrorMessage = nameof(ApiErrorCode.FieldRequired))]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = nameof(ApiErrorCode.FieldRequired))]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = nameof(ApiErrorCode.FieldRequired))]
        [Compare(nameof(NewPassword), ErrorMessage = nameof(ApiErrorCode.FieldMismatch))]
        public string NewPasswordVerify { get; set; }

        public string ReCaptcha { get; set; }
    }
}