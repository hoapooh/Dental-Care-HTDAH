using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dental_Clinic_System.Helper
{
	public static class HtmlHelpers
	{
		public static string GetDisplayName<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
		{
			var memberExpression = expression.Body as MemberExpression;
			if (memberExpression == null)
			{
				throw new ArgumentException("Expression must be a member expression", nameof(expression));
			}

			var property = memberExpression.Member as PropertyInfo;
			if (property == null)
			{
				throw new ArgumentException("Expression must target a property", nameof(expression));
			}

			var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
			if (displayNameAttribute != null)
			{
				return displayNameAttribute.DisplayName;
			}

			return property.Name; // Fallback to the property name if DisplayName is not set
		}

		//Xóa các thẻ Html ra khỏi chuỗi (string)
		public static string StripHtmlTags(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty;
			}

			return Regex.Replace(input, "<.*?>", string.Empty);
		}

        public static string ExtractTextAndLimitCharacters(string htmlContent, int maxLength)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return "";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var textContent = htmlDoc.DocumentNode.InnerText;

            // Truncate the text to the desired length
            if (textContent.Length > maxLength)
                return textContent.Substring(0, maxLength) + "...";

            return textContent;
        }
    }
}
