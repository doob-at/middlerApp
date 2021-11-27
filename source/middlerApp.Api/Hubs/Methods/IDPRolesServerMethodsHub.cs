using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using doob.SignalARRR.Common.Attributes;
using doob.SignalARRR.Server;
using Microsoft.AspNetCore.Authorization;
using middlerApp.Api.Hubs;
using middlerApp.Auth.Models.DTO;
using middlerApp.Auth.Services;
using middlerApp.Events;

namespace middlerApp.API.HubMethods
{
    [MessageName("IDPRoles")]
    public class IDPRolesServerMethodsHub : ServerMethods<UIHub>
    {
        public DataEventDispatcher EventDispatcher { get; }



        public IDPRolesServerMethodsHub(DataEventDispatcher eventDispatcher)
        {
            EventDispatcher = eventDispatcher;

        }


        public IObservable<DataEvent> Subscribe()
        {
            return EventDispatcher.Notifications.Where(ev => ev.Subject == "IDPRoles");
        }
    }
}
