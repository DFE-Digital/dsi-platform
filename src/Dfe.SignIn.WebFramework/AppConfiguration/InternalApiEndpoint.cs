using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.AppConfiguration;

public class InternalApiEndpoint
{
    [Required, Url]
    public string? BaseAddress { get; set; }
}
