using Dfe.SignIn.Core.Contracts.Graph;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// 
/// </summary>
public interface IGraphApiChangeUserPersonalDetails
{
    Task ChangeName(string forename, string lastName, GraphAccessToken? graphAccessToken);
}
