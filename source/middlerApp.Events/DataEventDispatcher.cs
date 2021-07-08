using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace middlerApp.Events
{
    public class DataEventDispatcher
    {
        
        private Subject<DataEvent> NotificationsSubject { get; } = new Subject<DataEvent>();
        public IObservable<DataEvent> Notifications => NotificationsSubject.Where(n => n != null).AsObservable();

        public void DispatchEvent(DataEvent @event)
        {
            NotificationsSubject.OnNext(@event);
        }

        public void DispatchEvent(DataEventAction action, string subject, object payload = null)
        {
            var ev = new DataEvent(action, subject, payload);
           DispatchEvent(ev);
        }

        public void DispatchCreatedEvent(string subject, object payload = null)
        {
            DispatchEvent(DataEvent.Created(subject, payload));
        }

        public void DispatchUpdatedEvent(string subject, object payload = null)
        {
            DispatchEvent(DataEvent.Updated(subject, payload));
        }

        public void DispatchDeletedEvent(string subject, object payload = null)
        {
            DispatchEvent(DataEvent.Deleted(subject, payload));
        }

    }
}
