using Dfe.SignIn.Core.Models.Organisations;

public static class MockOrganisations
{
    public static IDictionary<Guid, OrganisationModel> Models { get; } = new Dictionary<Guid, OrganisationModel> {
        { new Guid("df2b7f6e-d156-45f5-a092-49b8d9fdf4e0"), new OrganisationModel {
            Id = new Guid("df2b7f6e-d156-45f5-a092-49b8d9fdf4e0"),
            Name = "Organisation A",
            LegalName = "Organisation A",
        } },
        { new Guid("fd63379e-6214-41ef-82fd-77b565c2a4fe"), new OrganisationModel {
            Id = new Guid("fd63379e-6214-41ef-82fd-77b565c2a4fe"),
            Name = "Organisation B",
            LegalName = "Organisation B",
        } },
        { new Guid("763b713d-893b-4116-b6ef-7c88c14a5dd7"), new OrganisationModel {
            Id = new Guid("763b713d-893b-4116-b6ef-7c88c14a5dd7"),
            Name = "Organisation C",
            LegalName = "Organisation C",
        } },
    };
}
