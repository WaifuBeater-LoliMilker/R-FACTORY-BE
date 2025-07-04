namespace R_Factory_BE.Services
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipJWTMiddlewareAttribute : Attribute
    {
    }
}
