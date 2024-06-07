using Newtonsoft.Json.Linq;

namespace Dental_Clinic_System.Helper
{
	public class LocalAPIReverseString
	{
		private static readonly HttpClient client = new();

		public async Task<string> GetProvinceNameById(int provinceId)
		{
			string url = $"https://esgoo.net/api-tinhthanh/1/0.htm";
			var response = await client.GetStringAsync(url);
			var json = JObject.Parse(response);

			if (json["error"].ToString() == "0")
			{
				foreach (var province in json["data"])
				{
					if (province["id"].ToString() == provinceId.ToString())
					{
						return province["full_name"].ToString();
					}
				}
			}
			return "Not Found";
		}

		public async Task<string> GetDistrictNameById(int provinceId, int districtId)
		{
			string url = $"https://esgoo.net/api-tinhthanh/2/{provinceId}.htm";
			var response = await client.GetStringAsync(url);
			var json = JObject.Parse(response);

			if (json["error"].ToString() == "0")
			{
				foreach (var district in json["data"])
				{
					if (district["id"].ToString() == districtId.ToString())
					{
						return district["full_name"].ToString();
					}
				}
			}
			return "Not Found";
		}

		public async Task<string> GetWardNameById(int districtId, int wardId)
		{
			string url = $"https://esgoo.net/api-tinhthanh/3/{districtId}.htm";
			var response = await client.GetStringAsync(url);
			var json = JObject.Parse(response);

			if (json["error"].ToString() == "0")
			{
				foreach (var ward in json["data"])
				{
					if (ward["id"].ToString() == wardId.ToString())
					{
						return ward["full_name"].ToString();
					}
				}
			}
			return "Not Found";
		}
	}
}
