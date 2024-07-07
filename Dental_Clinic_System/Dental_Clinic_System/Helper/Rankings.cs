using System;
using System.Linq;
using System.Collections.Generic;
using Dental_Clinic_System.Models.Data;

public class Rankings
{
    private readonly DentalClinicDbContext _context;

    public Rankings(DentalClinicDbContext context)
    {
        _context = context;
    }

    public void GetClinicRankings(int clinicId, DateOnly startDate, DateOnly endDate)
    {
        var clinicDentists = _context.Dentists
            .Where(d => d.ClinicID == clinicId)
            .ToList();

        // Calculate highest and lowest rated dentists
        var dentistRatings = _context.Reviews
            .Where(r => r.Date >= startDate && r.Date <= endDate && clinicDentists.Select(d => d.ID).Contains(r.DentistID))
            .GroupBy(r => r.DentistID)
            .Select(g => new
            {
                DentistID = g.Key,
                AverageRating = g.Average(r => r.Rating),
                ReviewCount = g.Count()
            })
            .ToList();

        var highestRatedDentists = dentistRatings.OrderByDescending(x => x.AverageRating).ToList();
        var lowestRatedDentists = dentistRatings.OrderBy(x => x.AverageRating).ToList();

        // Calculate most and least booked dentists
        var dentistBookings = _context.Appointments
            .Where(a => a.Schedule.Date >= startDate && a.Schedule.Date <= endDate && clinicDentists.Select(d => d.ID).Contains(a.Schedule.DentistID))
            .GroupBy(a => a.Schedule.DentistID)
            .Select(g => new
            {
                DentistID = g.Key,
                BookingCount = g.Count()
            })
            .ToList();

        var mostBookedDentists = dentistBookings.OrderByDescending(x => x.BookingCount).ToList();
        var leastBookedDentists = dentistBookings.OrderBy(x => x.BookingCount).ToList();

        // Calculate highest and lowest rated specialties
        var specialtyRatings = _context.Appointments
            .Where(a => a.Schedule.Date >= startDate && a.Schedule.Date <= endDate && clinicDentists.Select(d => d.ID).Contains(a.Schedule.DentistID))
            .GroupBy(a => a.SpecialtyID)
            .Select(g => new
            {
                SpecialtyID = g.Key,
                AverageRating = g.Average(a => a.Schedule.Dentist.Reviews.Average(r => r.Rating)),
                BookingCount = g.Count()
            })
            .ToList();

        var highestRatedSpecialties = specialtyRatings.OrderByDescending(x => x.AverageRating).ToList();
        var lowestRatedSpecialties = specialtyRatings.OrderBy(x => x.AverageRating).ToList();

        var mostBookedSpecialties = specialtyRatings.OrderByDescending(x => x.BookingCount).ToList();
        var leastBookedSpecialties = specialtyRatings.OrderBy(x => x.BookingCount).ToList();

        // Calculate successful and canceled appointments
        var appointmentStats = GetAppointmentStats(clinicId, startDate, endDate);

        // Output the rankings and statistics for this clinic
        Console.WriteLine($"Rankings and Statistics for Clinic ID: {clinicId}");

        // Highest and lowest rated dentists
        Console.WriteLine("Highest Rated Dentists:");
        foreach (var rating in highestRatedDentists)
        {
            Console.WriteLine($"Dentist ID: {rating.DentistID}, Average Rating: {rating.AverageRating}, Review Count: {rating.ReviewCount}");
        }

        Console.WriteLine("Lowest Rated Dentists:");
        foreach (var rating in lowestRatedDentists)
        {
            Console.WriteLine($"Dentist ID: {rating.DentistID}, Average Rating: {rating.AverageRating}, Review Count: {rating.ReviewCount}");
        }

        // Most and least booked dentists
        Console.WriteLine("Most Booked Dentists:");
        foreach (var booking in mostBookedDentists)
        {
            Console.WriteLine($"Dentist ID: {booking.DentistID}, Booking Count: {booking.BookingCount}");
        }

        Console.WriteLine("Least Booked Dentists:");
        foreach (var booking in leastBookedDentists)
        {
            Console.WriteLine($"Dentist ID: {booking.DentistID}, Booking Count: {booking.BookingCount}");
        }

        // Highest and lowest rated specialties
        Console.WriteLine("Highest Rated Specialties:");
        foreach (var rating in highestRatedSpecialties)
        {
            Console.WriteLine($"Specialty ID: {rating.SpecialtyID}, Average Rating: {rating.AverageRating}, Booking Count: {rating.BookingCount}");
        }

        Console.WriteLine("Lowest Rated Specialties:");
        foreach (var rating in lowestRatedSpecialties)
        {
            Console.WriteLine($"Specialty ID: {rating.SpecialtyID}, Average Rating: {rating.AverageRating}, Booking Count: {rating.BookingCount}");
        }

        // Most and least booked specialties
        Console.WriteLine("Most Booked Specialties:");
        foreach (var specialty in mostBookedSpecialties)
        {
            Console.WriteLine($"Specialty ID: {specialty.SpecialtyID}, Booking Count: {specialty.BookingCount}, Average Rating: {specialty.AverageRating}");
        }

        Console.WriteLine("Least Booked Specialties:");
        foreach (var specialty in leastBookedSpecialties)
        {
            Console.WriteLine($"Specialty ID: {specialty.SpecialtyID}, Booking Count: {specialty.BookingCount}, Average Rating: {specialty.AverageRating}");
        }

        // Successful and canceled appointments
        Console.WriteLine($"Successful Appointments: {appointmentStats[0]}");
        Console.WriteLine($"Canceled Appointments: {appointmentStats[1]}");
    }

    public List<int> GetAppointmentStats(int clinicId, DateOnly startDate, DateOnly endDate)
    {
        var successfulAppointments = _context.Appointments
            .Where(a => a.AppointmentStatus == "Đã Khám" && a.Schedule.Date >= startDate && a.Schedule.Date <= endDate && a.Schedule.Dentist.ClinicID == clinicId)
            .Count();

        var canceledAppointments = _context.Appointments
            .Where(a => a.AppointmentStatus == "Đã Hủy" && a.Schedule.Date >= startDate && a.Schedule.Date <= endDate && a.Schedule.Dentist.ClinicID == clinicId)
            .Count();

        return new List<int> { successfulAppointments, canceledAppointments };
    }
}
