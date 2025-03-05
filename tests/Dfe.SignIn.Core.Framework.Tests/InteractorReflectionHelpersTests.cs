using System.Reflection;
using Dfe.SignIn.Core.Framework.Tests.Fakes;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.Framework.Tests;

[TestClass]
public sealed class InteractorReflectionHelpersTests
{
    private static readonly Assembly TestAssembly = typeof(InteractorReflectionHelpersTests).Assembly;

    #region DiscoverInteractorTypesInAssembly(Assembly)

    [TestMethod]
    public void DiscoverInteractorTypesInAssembly_Throws_WhenAssemblyArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void DiscoverInteractorTypesInAssembly_ReturnsExpectedTypes()
    {
        var interactorTypes = InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(TestAssembly)
            .ToArray();

        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest, ExampleResponse>),
            ConcreteType = typeof(Example_UseCaseHandler),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>),
            ConcreteType = typeof(AnotherExample_UseCaseHandler),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest, ExampleResponse>),
            ConcreteType = typeof(Example_ApiRequester),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>),
            ConcreteType = typeof(AnotherExample_ApiRequester),
        });
    }

    #endregion

    #region DiscoverUseCaseHandlerTypesInAssembly(Assembly)

    [TestMethod]
    public void DiscoverUseCaseHandlerTypesInAssembly_Throws_WhenAssemblyArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => InteractorReflectionHelpers.DiscoverUseCaseHandlerTypesInAssembly(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void DiscoverUseCaseHandlerTypesInAssembly_ReturnsExpectedTypes()
    {
        var interactorTypes = InteractorReflectionHelpers.DiscoverUseCaseHandlerTypesInAssembly(TestAssembly)
            .ToArray();

        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest, ExampleResponse>),
            ConcreteType = typeof(Example_UseCaseHandler),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>),
            ConcreteType = typeof(AnotherExample_UseCaseHandler),
        });
    }

    #endregion

    #region DiscoverApiRequesterTypesInAssembly(Assembly)

    [TestMethod]
    public void DiscoverApiRequesterTypesInAssembly_Throws_WhenAssemblyArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void DiscoverApiRequesterTypesInAssembly_ReturnsExpectedTypes()
    {
        var interactorTypes = InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(TestAssembly)
            .ToArray();

        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest, ExampleResponse>),
            ConcreteType = typeof(Example_ApiRequester),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest, AnotherExampleResponse>),
            ConcreteType = typeof(AnotherExample_ApiRequester),
        });
    }

    #endregion

    #region Unit testing

    [TestMethod]
    public void UnitTesting_CanEasilyInjectMockInteractors()
    {
        var autoMocker = new AutoMocker();

        var service = autoMocker.CreateInstance<FakeServiceWithInteraction>();

        Assert.IsNotNull(service.Interaction);
    }

    #endregion
}
