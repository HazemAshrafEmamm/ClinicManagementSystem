using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Models
{
    public class DoctorSchedule
    {
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required(ErrorMessage = "يرجى تحديد اليوم")]
        [Display(Name = "اليوم")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "يرجى تحديد وقت البدء")]
        [Display(Name = "من الساعة")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "يرجى تحديد وقت الانتهاء")]
        [Display(Name = "إلى الساعة")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }
}
