using Newtonsoft.Json;

namespace Dental_Clinic_System.Areas.Dentist.Helper
{
	public class DateOnlyConverter : JsonConverter<DateOnly>
	{
		private readonly string _format = "MM/dd/yyyy"; // This should match the format of your dates

		public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString(_format));
		}

		public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return DateOnly.ParseExact((string)reader.Value, _format, null);
		}
	}

}
