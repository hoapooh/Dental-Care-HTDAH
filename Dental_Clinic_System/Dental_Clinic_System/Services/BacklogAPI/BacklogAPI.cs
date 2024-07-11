using Dental_Clinic_System.Helper;

namespace Dental_Clinic_System.Services.BacklogAPI
{
    public class BacklogAPI : IBacklogAPI
    {
        public async Task GetSpaceInfoAsync()
        {
            string backlogApiUrl = "https://rivinger.backlog.com/api/v2/space?apiKey=a4sf65GwuS7fqvUhebSZOt0ZgLUH8ZrCt8DmIpwddcx90iHQkvsRC5ekAgjGw7lm";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(backlogApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    //Console.WriteLine("==================================================");
                    //Console.WriteLine($"Status code: {response.StatusCode}");
                    //Console.WriteLine("==================================================");

                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    //Console.WriteLine($"Failed to get space info. Status code: {response.StatusCode}");
                }
            }
        }

        public async Task SendErrorToWebhookAsync(string subject, string description, string categoryID)
        {
            // Category ID
            // Login = 153744
            // Schedule = 153745
            // EmailSender = 153746
            // UI = 153747
            // Booking = 153748
            // Firebase = 153750
            // Searching = 153751
            // Register = 153752
            // APIKey = 153753
            // Profile = 153853
            // Cancel Appointment = 153854

            string backlogApiUrl = "https://rivinger.backlog.com/api/v2/issues?apiKey=a4sf65GwuS7fqvUhebSZOt0ZgLUH8ZrCt8DmIpwddcx90iHQkvsRC5ekAgjGw7lm";

            var newIssue = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("projectId", "135037"),
                new KeyValuePair<string, string>("summary", subject),
                new KeyValuePair<string, string>("description", description),
                new KeyValuePair<string, string>("issueTypeId", "575393"),
                new KeyValuePair<string, string>("priorityId", "2"),
                new KeyValuePair<string, string>("categoryId[]", categoryID),
                new KeyValuePair<string, string>("milestoneId[]", "125475"),
                new KeyValuePair<string, string>("startDate", DateOnly.FromDateTime(Util.GetUtcPlus7Time()).ToString("yyyy-MM-dd")),
                //new KeyValuePair<string, string>("dueDate", "2024-07-24"),
                //new KeyValuePair<string, string>("estimatedHours", "12")
            });

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(backlogApiUrl, newIssue);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("===============================================================");
                    Console.WriteLine($"Failed to create issue. Status code: {response.StatusCode}");
                    Console.WriteLine("===============================================================");

                }
                Console.WriteLine("===============================================================");
                Console.WriteLine($"Failed to create issue. Status code: {response.StatusCode}");
                Console.WriteLine("===============================================================");
            }
        }
    }
}
