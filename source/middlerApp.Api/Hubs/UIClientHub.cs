using System;
using doob.SignalARRR.Server;

namespace middlerApp.Api.Hubs
{
    public class UIHub: HARRR
    {
        public UIHub(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
