using System;
using System.Collections.Generic;

namespace ClinicManagementSystem.Models.ViewModels
{
    public class DoctorDashboardViewModel
    {
        public int ConfirmedAppointmentsCount { get; set; }
        public int CompletedAppointmentsCount { get; set; }
        
        // Key: DayOfWeek, Value: List of appointments on that day
        public Dictionary<DayOfWeek, List<Appointment>> AppointmentsByDay { get; set; } = new Dictionary<DayOfWeek, List<Appointment>>();
        
        public List<DayOfWeek> ScheduledDays { get; set; } = new List<DayOfWeek>();
    }
}
