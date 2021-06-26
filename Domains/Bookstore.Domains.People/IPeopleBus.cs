using MassTransit;

namespace Bookstore.Domains.People
{
    // This is a well-known convention for supporting named dependencies in a container that doesn't do so natively.
    // MassTransit supports this convention and will create an IBusControl tagged with the appropriate interface, allowing
    // one to inject an IBusControl<IXXXBus> instead of the non-generic IBusControl. The net result is that we can choose
    // exactly which bus control we want simply by injecting an IBusControl<T> of the correct type.
    public interface IPeopleBus : IBus
    {
    }
}