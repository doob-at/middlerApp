using System;
using System.Reactive.Linq;
using doob.SignalARRR.Common.Attributes;
using doob.SignalARRR.Server;
using middlerApp.Events;

namespace middlerApp.Api.Hubs.Methods {

    [MessageName("EndpointRules")]
    public class EndpointRulesHubMethods: ServerMethods<UIHub> {
        
        public DataEventDispatcher EventDispatcher { get; }

        public EndpointRulesHubMethods(DataEventDispatcher eventDispatcher)
        {
            EventDispatcher = eventDispatcher;
        }

        
        public IObservable<object> Subscribe() {

            return EventDispatcher.Notifications.Where(ev => ev.Subject == "EndpointRules");

        }

    }
}
