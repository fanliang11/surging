namespace Surging.Core.CPlatform.Support
{
    public class ServiceCommandDescriptor:ServiceCommand
    {
        public string ServiceId { get; set; }


        public override bool Equals(object obj)
        {
            var model = obj as ServiceCommandDescriptor;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;
             
            return FailoverCluster == model.FailoverCluster && CircuitBreakerForceOpen == model.CircuitBreakerForceOpen &&
            Strategy == model.Strategy &&
            ExecutionTimeoutInMilliseconds == model.ExecutionTimeoutInMilliseconds &&
            Injection == model.Injection &&
            InjectionNamespaces == model.InjectionNamespaces &&
            BreakeErrorThresholdPercentage == model.BreakeErrorThresholdPercentage &&
            BreakeSleepWindowInMilliseconds == model.BreakeSleepWindowInMilliseconds &&
            BreakerForceClosed == model.BreakerForceClosed &&
            BreakerRequestVolumeThreshold == model.BreakerRequestVolumeThreshold &&
            MaxConcurrentRequests == model.MaxConcurrentRequests &&
            FallBackName == model.FallBackName && ShuntStrategy == model.ShuntStrategy;
        }
    }
}
