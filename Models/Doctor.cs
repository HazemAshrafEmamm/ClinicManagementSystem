using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "حقل الاسم مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب ألا يتجاوز 100 حرف")]
        [Display(Name = "اسم الطبيب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "حقل الهاتف مطلوب")]
        [StringLength(20, ErrorMessage = "رقم الهاتف يجب ألا يتجاوز 20 حرف")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [StringLength(150, ErrorMessage = "البريد الإلكتروني يجب ألا يتجاوز 150 حرف")]
        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "يرجى اختيار التخصص")]
        [Display(Name = "التخصص")]
        public int SpecialtyId { get; set; }

        public Specialty? Specialty { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    }
}
