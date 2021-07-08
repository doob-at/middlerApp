using System;

namespace middlerApp.SharedModels
{
    public class EndpointRuleListActionDto
    {
        public Guid Id { get; set; }
        public virtual bool Enabled { get; set; }
        public string ActionType { get; set; }

        public bool Terminating { get; set; }
    }
}
