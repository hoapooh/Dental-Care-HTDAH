using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DentalCareUnitTest
{
    public class DBContextTest
    {
        private DentalClinicDbContext _context;

        public DBContextTest()
        {
            
        }

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DentalClinicDbContext>()
                .UseInMemoryDatabase(databaseName: "dentalcareplatformdb")
                .Options;

            _context = new DentalClinicDbContext(options);
        }

        private void GetAccount()
        {
            _context.Accounts.Add(new Account { Username = "admin", Password = DataEncryptionExtensions.ToMd5Hash("D123"), Role = "Admin", AccountStatus = "Hoạt Động" });
            _context.SaveChanges();
        }

        [Test]
        [TestCase("admin", "D123")]
        public void Authenticate_ShouldReturnAdminAccount(string username, string password)
        {
            // Act
            GetAccount(); // Invoke GettAccount Method
            var account = _context.Accounts.SingleOrDefault(a => a.Username == username && a.Password  == DataEncryptionExtensions.ToMd5Hash(password));

            // Assert
            Assert.IsNotNull(account);
            Assert.AreEqual("Admin", account.Role);
        }

        [TearDown]
        public void TearDown()
        {
            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }
}
