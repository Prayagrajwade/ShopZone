namespace ShopAPI.Common.Email
{
    public static class EmailTemplateHelper
    {
        public static string Replace(string template, Dictionary<string, string> data)
        {
            foreach (var key in data.Keys)
            {
                template = template.Replace($"{{{{{key}}}}}", data[key]);
            }
            return template;
        }
    }
}
