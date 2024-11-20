using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace Surging.Core.Protocol.Udp.Runtime
{
   public class UdpServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }

        public Func<UdpBehavior> Behavior { get; set; }

        public  ISubject<UdpBehavior> BehaviorSubject { get; set; }=new ReplaySubject<UdpBehavior>();
    }

}
