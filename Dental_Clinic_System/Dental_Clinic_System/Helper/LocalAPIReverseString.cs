using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dental_Clinic_System.Helper
{
    public static class LocalAPIReverseString
    {
        private static readonly HttpClient client = new();

        private static string FormatId(int id, int length)
        {
            return id.ToString().PadLeft(length, '0');
        }

        public static async Task<string> GetProvinceNameById(int provinceId)
        {
            string url = $"https://esgoo.net/api-tinhthanh/1/0.htm";
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["error"].ToString() == "0")
            {
                string formattedProvinceId = FormatId(provinceId, 2);
                foreach (var province in json["data"])
                {
                    if (province["id"].ToString() == formattedProvinceId)
                    {
                        return province["full_name"].ToString();
                    }
                }
            }
            return "Not Found";
        }

        public static async Task<string> GetDistrictNameById(int provinceId, int districtId)
        {
            string formattedProvinceId = FormatId(provinceId, 2);
            string url = $"https://esgoo.net/api-tinhthanh/2/{formattedProvinceId}.htm";
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["error"].ToString() == "0")
            {
                string formattedDistrictId = FormatId(districtId, 3);
                foreach (var district in json["data"])
                {
                    if (district["id"].ToString() == formattedDistrictId)
                    {
                        return district["full_name"].ToString();
                    }
                }
            }
            return "Not Found";
        }

        public static async Task<string> GetWardNameById(int districtId, int wardId)
        {
            string formattedDistrictId = FormatId(districtId, 3);
            string url = $"https://esgoo.net/api-tinhthanh/3/{formattedDistrictId}.htm";
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["error"].ToString() == "0")
            {
                string formattedWardId = FormatId(wardId, 5);
                foreach (var ward in json["data"])
                {
                    if (ward["id"].ToString() == formattedWardId)
                    {
                        return ward["full_name"].ToString();
                    }
                }
            }
            return "Not Found";
        }
    }
}
