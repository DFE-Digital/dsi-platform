using Dfe.SignIn.Core.Models.Organisations;

public static class MockOrganisations
{
    public static IDictionary<Guid, OrganisationModel> Models { get; } = new Dictionary<Guid, OrganisationModel> {
        { new Guid("a4412d34-6471-4663-8d70-73fe6617b5e5"), new OrganisationModel {
            Id = new Guid("a4412d34-6471-4663-8d70-73fe6617b5e5"),
            Name = "Organisation A",
            LegalName = "Legal name A",
            Status = 1,
        } },
        { new Guid("561cdabf-d2f8-48f3-a66b-0f943837c9d7"), new OrganisationModel {
            Id = new Guid("561cdabf-d2f8-48f3-a66b-0f943837c9d7"),
            Name = "Organisation B",
            LegalName = "Legal name B",
            Status = 1,
        } },
        { new Guid("763b713d-893b-4116-b6ef-7c88c14a5dd7"), new OrganisationModel {
            Id = new Guid("763b713d-893b-4116-b6ef-7c88c14a5dd7"),
            Name = "Organisation C",
            LegalName = "Organisation C",
            Status = 1,
        } },
    };
}
