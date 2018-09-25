using DTO.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.Auth.Dto
{
    public class LoginReq : BaseDto
    {
        public string UserName { get; set; }
        public string Pwd { get; set; }
        public Guid CorporationKeyId { get; set; }
    }
}
