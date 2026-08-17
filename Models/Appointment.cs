using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "تاريخ ووقت الموعد مطلوب")]
        [Display(Name = "تاريخ الموعد")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "المدة (بالدقائق)")]
        public int Duration { get; set; } = 30;

        [Display(Name = "حالة الموعد")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "يرجى اختيار المريض")]
        [Display(Name = "المريض")]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الطبيب")]
        [Display(Name = "الطبيب")]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Display(Name = "هل تم الدفع؟")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "طريقة الدفع")]
        public string? PaymentMethod { get; set; }
    }
}
