using Repository.EF.Core;
using Surging.Core.ProxyGenerator;
using System;

namespace App.Core
{
   public class  BaseAppService : ProxyServiceBase
    {
      //  protected BaseImpRepository _repository;
        //protected QueryOrgRepository _queryOnlyRepository;
        public BaseAppService()//(BaseImpRepository repository)//, QueryOrgRepository queryOnlyRepository)
        {
           // _repository = repository;
            //_queryOnlyRepository = queryOnlyRepository;
        }
    }
}
