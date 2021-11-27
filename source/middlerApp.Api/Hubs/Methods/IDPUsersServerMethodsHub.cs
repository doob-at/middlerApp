using System;
using System.Reactive.Linq;
using doob.SignalARRR.Common.Attributes;
using doob.SignalARRR.Server;
using middlerApp.Events;

namespace middlerApp.Api.Hubs.Methods
{
    [MessageName("IDPUsers")]
    public class IDPUsersServerMethodsHub : ServerMethods<UIHub>
    {
        public DataEventDispatcher EventDispatcher { get; }


        public IDPUsersServerMethodsHub(DataEventDispatcher eventDispatcher)
        {
            EventDispatcher = eventDispatcher;

        }


        public IObservable<DataEvent> Subscribe()
        {
            return EventDispatcher.Notifications.Select(ev =>
            {
                var z = ev;
                return z;
            }).Where(ev => ev.Subject == "IDPUsers");
        }
    }
}
