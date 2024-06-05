﻿namespace Dental_Clinic_System.ViewModels
{
	public class PatientVM
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Gender { get; set; }
		public string Email { get; set; }
		public DateOnly? DateOfBirth { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? Address { get; set; }
	}
}
