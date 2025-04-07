using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AllHoursCafe.API.Data
{
    public class UpdateDbSeederImageUrls
    {
        public static void UpdateImageUrls()
        {
            string filePath = "AllHoursCafe.API/Data/DbSeeder.cs";
            string content = File.ReadAllText(filePath);

            // Update image URLs
            content = Regex.Replace(
                content,
                @"ImageUrl = ""/images/menu/([^""]+)\.jpg"",",
                match =>
                {
                    string fileName = match.Groups[1].Value;
                    string category = GetCategoryFromContext(content, match.Index);
                    return $"ImageUrl = \"/images/Items/{category.ToLower()}/{fileName}.jpg\",";
                }
            );

            File.WriteAllText(filePath, content);
            Console.WriteLine("Updated image URLs in DbSeeder.cs");
        }

        private static string GetCategoryFromContext(string content, int position)
        {
            // Look for CategoryId = X nearby
            int startPos = Math.Max(0, position - 200);
            int endPos = Math.Min(content.Length, position + 200);
            string context = content.Substring(startPos, endPos - startPos);

            var match = Regex.Match(context, @"CategoryId = (\d+),.*?// (\w+)");
            if (match.Success)
            {
                return match.Groups[2].Value;
            }

            // Default to a category based on position in file
            if (context.Contains("breakfastItems"))
                return "breakfast";
            if (context.Contains("lunchItems"))
                return "lunch";
            if (context.Contains("dinnerItems"))
                return "dinner";
            if (context.Contains("beverageItems"))
                return "beverages";
            if (context.Contains("dessertItems"))
                return "desserts";
            if (context.Contains("snackItems"))
                return "snacks";

            return "unknown";
        }
    }
}
