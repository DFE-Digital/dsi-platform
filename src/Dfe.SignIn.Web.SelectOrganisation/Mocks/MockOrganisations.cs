using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.Web.SelectOrganisation.Mocks;

public static class MockOrganisations
{
    public static IDictionary<Guid, Organisation> Models { get; } = new Dictionary<Guid, Organisation> {
        { new Guid("a4412d34-6471-4663-8d70-73fe6617b5e5"), new Organisation {
            Id = new Guid("a4412d34-6471-4663-8d70-73fe6617b5e5"),
            Name = "Organisation A",
            LegalName = "Legal name A",
            Status = OrganisationStatus.Open,
        } },
        { new Guid("561cdabf-d2f8-48f3-a66b-0f943837c9d7"), new Organisation {
            Id = new Guid("561cdabf-d2f8-48f3-a66b-0f943837c9d7"),
            Name = "Organisation B",
            LegalName = "Legal name B",
            Status = OrganisationStatus.Open,
        } },
        { new Guid("763b713d-893b-4116-b6ef-7c88c14a5dd7"), new Organisation {
            Id = new Guid("763b713d-893b-4116-b6ef-7c88c14a5dd7"),
            Name = "Organisation C",
            LegalName = "Organisation C",
            Status = OrganisationStatus.Open,
        } },
    };
}
