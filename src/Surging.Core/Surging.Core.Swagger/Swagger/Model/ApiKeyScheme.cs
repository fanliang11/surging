namespace Surging.Core.Swagger
{
    public class ApiKeyScheme : SecurityScheme
    {
        public string Name { get; set; }

        public string In { get; set; }

        public ApiKeyScheme()
        {
            Type = "apiKey";
        }
    }
}
