using DDD.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace  Domain.Org.ValueObject
{
   public class EmployeeRole: BaseValueObject
    {
        [ForeignKey("Employee")]
        public Guid EmployeeKeyId { get; set; }
        
    }
}
