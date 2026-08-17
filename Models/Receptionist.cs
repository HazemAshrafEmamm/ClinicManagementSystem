using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Models
{
    public class Receptionist
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "حقل الاسم مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب ألا يتجاوز 100 حرف")]
        [Display(Name = "اسم موظف الاستقبال")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "حقل الهاتف مطلوب")]
        [StringLength(20, ErrorMessage = "رقم الهاتف يجب ألا يتجاوز 20 حرف")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [Required(ErrorMessage = "حقل البريد الإلكتروني مطلوب")]
        [StringLength(150, ErrorMessage = "البريد الإلكتروني يجب ألا يتجاوز 150 حرف")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "مستخدم النظام")]
        public string? UserId { get; set; }
    }
}
