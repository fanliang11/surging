using DDD.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.ValueObject.Inquiry
{
    
  public class InquiryParameter: BaseValueObject
    {
       // [Column("InquiryKeyId")]
        public Guid InquiryKeyId { get; set; }
     //  [ForeignKey("InquiryKeyId")]
      //  public CusInquiry OwnCusInquiry { get; set; }

        public Guid SysParameterItemKeyId { get; set; } 
        public int ParameterType { get; set; } 
        public int Seq { get; set; }
        /*
         [InverseProperty("Author")]
     public List<Post> AuthoredPosts { get; set; }

     [InverseProperty("Contributor")]
     public List<Post> ContributedToPosts { get; set; }
          */

    }
}
