using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dfe.SignIn.Base.Framework.UnitTests.Fakes;
using Moq.AutoMock;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class InteractorReflectionHelpersTests
{
    private static readonly Assembly TestAssembly = typeof(InteractorReflectionHelpersTests).Assembly;

    #region DiscoverInteractorTypesInAssembly(Assembly)

    [TestMethod]
    public void DiscoverInteractorTypesInAssembly_Throws_WhenAssemblyArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(
                assembly: null!
            ));
    }

    [TestMethod]
    public void DiscoverInteractorTypesInAssembly_ReturnsExpectedTypes()
    {
        var interactorTypes = InteractorReflectionHelpers.DiscoverInteractorTypesInAssembly(TestAssembly)
            .ToArray();

        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = typeof(ExampleUseCase),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest>),
            ConcreteType = typeof(AnotherExampleUseCase),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = typeof(Example_ApiRequester),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest>),
            ConcreteType = typeof(AnotherExample_ApiRequester),
        });
    }

    #endregion

    #region DiscoverUseCaseHandlerTypesInAssembly(Assembly)

    [TestMethod]
    public void DiscoverUseCaseHandlerTypesInAssembly_Throws_WhenAssemblyArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractorReflectionHelpers.DiscoverUseCaseHandlerTypesInAssembly(
                assembly: null!
            ));
    }

    [TestMethod]
    public void DiscoverUseCaseHandlerTypesInAssembly_ReturnsExpectedTypes()
    {
        var interactorTypes = InteractorReflectionHelpers.DiscoverUseCaseHandlerTypesInAssembly(TestAssembly)
            .ToArray();

        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = typeof(ExampleUseCase),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest>),
            ConcreteType = typeof(AnotherExampleUseCase),
        });
    }

    #endregion

    #region DiscoverApiRequesterTypesInAssembly(Assembly)

    [TestMethod]
    public void DiscoverApiRequesterTypesInAssembly_Throws_WhenAssemblyArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(
                assembly: null!
            ));
    }

    [TestMethod]
    public void DiscoverApiRequesterTypesInAssembly_ReturnsExpectedTypes()
    {
        var interactorTypes = InteractorReflectionHelpers.DiscoverApiRequesterTypesInAssembly(TestAssembly)
            .ToArray();

        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<ExampleRequest>),
            ConcreteType = typeof(Example_ApiRequester),
        });
        CollectionAssert.Contains(interactorTypes, new InteractorTypeDescriptor {
            ContractType = typeof(IInteractor<AnotherExampleRequest>),
            ConcreteType = typeof(AnotherExample_ApiRequester),
        });
    }

    #endregion

    #region Unit testing

    [TestMethod]
    [SuppressMessage("roslyn", "MSTEST0032",
        Justification = "Required for unit test."
    )]
    public void UnitTesting_CanEasilyInjectMockInteractors()
    {
        var autoMocker = new AutoMocker();

        var service = autoMocker.CreateInstance<FakeServiceWithInteraction>();

        Assert.IsNotNull(service.Interaction);
    }

    #endregion
}
