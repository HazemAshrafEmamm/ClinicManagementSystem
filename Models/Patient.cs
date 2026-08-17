using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "حقل الاسم مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب ألا يتجاوز 100 حرف")]
        [Display(Name = "اسم المريض")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "حقل الهاتف مطلوب")]
        [StringLength(20, ErrorMessage = "رقم الهاتف يجب ألا يتجاوز 20 حرف")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "العمر يجب أن يكون بين 1 و 120")]
        [Display(Name = "العمر")]
        public int? Age { get; set; }

        [Display(Name = "الجنس")]
        public Gender Gender { get; set; }

        [Display(Name = "مستخدم النظام")]
        public string? UserId { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
