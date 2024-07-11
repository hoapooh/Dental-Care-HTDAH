using NUnit.Framework;
using System.Collections.Generic;

namespace DentalCareUnitTest
{
    public class AppointmentManager
    {
        private List<string> appointments;

        public AppointmentManager()
        {
            appointments = new List<string>();
        }

        public void AddAppointment(string appointment)
        {
            appointments.Add(appointment);
        }

        public void CancelAppointment(string appointment)
        {
            appointments.Remove(appointment);
        }

        public List<string> GetAppointments()
        {
            return new List<string>(appointments);
        }
    }

    public class Tests
    {
        private AppointmentManager appointmentManager;

        [SetUp]
        public void Setup()
        {
            appointmentManager = new AppointmentManager();
        }

        [Test]
        public void AddAppointment_ShouldIncreaseAppointmentCount()
        {
            // Arrange
            var initialCount = appointmentManager.GetAppointments().Count;

            // Act
            appointmentManager.AddAppointment("Appointment 1");
            var newCount = appointmentManager.GetAppointments().Count;

            // Assert
            Assert.AreEqual(initialCount + 1, newCount);
        }

        [Test]
        public void CancelAppointment_ShouldDecreaseAppointmentCount()
        {
            // Arrange
            appointmentManager.AddAppointment("Appointment 1");
            var initialCount = appointmentManager.GetAppointments().Count;

            // Act
            appointmentManager.CancelAppointment("Appointment 1");
            var newCount = appointmentManager.GetAppointments().Count;

            // Assert
            Assert.AreEqual(initialCount - 1, newCount);
        }

        [Test]
        public void GetAppointments_ShouldReturnAllAddedAppointments()
        {
            // Arrange
            appointmentManager.AddAppointment("Appointment 1");
            appointmentManager.AddAppointment("Appointment 2");

            // Act
            var appointments = appointmentManager.GetAppointments();

            // Assert
            CollectionAssert.Contains(appointments, "Appointment 1");
            CollectionAssert.Contains(appointments, "Appointment 2");
        }

        [Test]
        public void CancelNonExistentAppointment_ShouldNotChangeAppointmentCount()
        {
            // Arrange
            appointmentManager.AddAppointment("Appointment 1");
            var initialCount = appointmentManager.GetAppointments().Count;

            // Act
            appointmentManager.CancelAppointment("NonExistentAppointment");
            var newCount = appointmentManager.GetAppointments().Count;

            // Assert
            Assert.AreEqual(initialCount, newCount);
        }
    }
}
