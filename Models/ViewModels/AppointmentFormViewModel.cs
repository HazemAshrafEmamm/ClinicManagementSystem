using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClinicManagementSystem.Models.ViewModels
{
    public class AppointmentFormViewModel
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

        [Required(ErrorMessage = "يرجى اختيار الطبيب")]
        [Display(Name = "الطبيب")]
        public int DoctorId { get; set; }

        // Select lists for dropdowns
        public IEnumerable<SelectListItem> Patients { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();

        // Predefined list of durations
        public IEnumerable<SelectListItem> Durations { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "15", Text = "15 دقيقة" },
            new SelectListItem { Value = "30", Text = "30 دقيقة" },
            new SelectListItem { Value = "45", Text = "45 دقيقة" },
            new SelectListItem { Value = "60", Text = "60 دقيقة" }
        };
    }
}
