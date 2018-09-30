namespace Surging.Core.Swagger
{
    public class BasicAuthScheme : SecurityScheme
    {
        public BasicAuthScheme()
        {
            Type = "basic";
        }
    }
}
