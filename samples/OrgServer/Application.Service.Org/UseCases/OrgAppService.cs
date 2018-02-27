using App.Core;
using Application.Interface.Org;
using Domain.Org.Entity;
using DTO.Core;
using Repository.EF.Core;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.ProxyGenerator;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Org.Aggregate;
using Application.Interface.Org.EVENT;
using Repository.Org;

namespace Application.Service.Org
{
    [ModuleName("Org")]
    public partial class OrgAppService : BaseAppService, IOrgAppService
    {
        private readonly OrgRepository _repository;
        private readonly CorpQueryRepository _queryCorpRepository;
        private readonly OrgQueryRepository _queryOrgRepository;
        private readonly EmployeeQueryRepository _queryEmployeeRepository;

        //  private ICacheProvider _cacheProvider;
        public OrgAppService(OrgRepository repository, OrgQueryRepository queryOrgRepository, CorpQueryRepository queryCorpRepository, EmployeeQueryRepository queryEmployeeRepository)
        // : base(repository)//, queryOnlyRepository)
        {
            _repository = repository;
            _queryCorpRepository = queryCorpRepository;
            _queryOrgRepository = queryOrgRepository;
            _queryEmployeeRepository = queryEmployeeRepository;
        }
        #region 公司

        public Task<OperateResultRsp> RegisterCorporation(CorpEditReq req)
        {
            //是否已经被注册？
            var existCorp = _queryCorpRepository.Exist(a => !a.IsDelete && a.Name == req.CorpName);
            if (!existCorp)
            {
                var aCorp = new Corporation
                {
                    CorporationKeyId = Guid.NewGuid(),
                    Name = req.CorpName,
                    No = req.CorpName,
                    Version = 1,
                    CreateTime = DateTime.Now,
                    UpdateTime = DateTime.Now,
                    IsDelete = false
                };
                _repository.Add(aCorp);
                _repository.Commit();
                return Task.FromResult(new OperateResultRsp
                {
                    OperateFlag = true,
                    OperateResult = "注册成功"
                });
            }
            else
            {
                return Task.FromResult(new OperateResultRsp
                {
                    OperateFlag = false,
                    OperateResult = "该企业已经被注册"
                });
            }

        }

        public Task<OperateResultRsp> ActivateCorporation(CommonCMDReq req)
        {
            var rsp = new OperateResultRsp
            {
                OperateFlag = false,
                OperateResult = "参数错误"
            };
            try
            {
                if (!string.IsNullOrEmpty(req.CommonCMD))
                {
                    var corp = _queryCorpRepository.Get(a => a.CorporationKeyId == req.Identify.CorporationKeyId).FirstOrDefault();
                    corp.IsDelete = false;
                    var empId = corp.Activate();

                    _repository.Commit();
                    rsp.OperateFlag = true;
                    rsp.OperateResult = "激活成功";
                    Publish(new CorporationActivatedEvent()
                    {
                        CorpId = req.Identify.CorporationKeyId,
                        Email = req.CommonCMD,
                        EmpId = empId
                    });
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return Task.FromResult(rsp);

            // return rsp;
        }

        public Task<OperateResultRsp> EditCorporation(CorpEditReq req)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 部门

        public Task<OperateResultRsp> CreateDepartment(DeptEditReq req)
        {
            if (req.CorporationKeyId != Guid.Empty)
            {
                var corp = _repository.FindBy(req.CorporationKeyId);
                if (corp != null)
                {
                    corp.Departments.Add(new Department()
                    {
                        Name = req.DepartmentName,
                        No = req.DepartmentNo,
                        DepartmentType = DepartmentCategory.Bizz,
                        ParentKeyId = req.ParentDeptKeyId.GetValueOrDefault(),
                        Version = 1,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now,
                        IsDelete = false
                    });
                }
                else
                {

                }
            }
            else
            {
                return Task.FromResult(new OperateResultRsp
                {
                    OperateFlag = false,
                    OperateResult = string.Empty
                });
            }
            _repository.Commit();
            return Task.FromResult(new OperateResultRsp
            {
                OperateFlag = true,
                OperateResult = string.Empty
            });
        }



        public Task<OperateResultRsp> ModifyDepartment(DeptEditReq req)
        {
            var corp = _repository.FindBy(req.CorporationKeyId);
            if (corp != null)
            {
                var dept = corp.Departments.FirstOrDefault(c => c.KeyId == req.DepartmentKeyId.Value);
                if (dept != null)
                {
                    dept.Name = req.DepartmentName;
                    dept.No = req.DepartmentNo;
                }
            }
            _repository.Commit();
            return Task.FromResult(new OperateResultRsp
            {
                OperateFlag = true,
                OperateResult = string.Empty
            });

        }



        public Task<OperateResultRsp> RemoveDepartment(KeyIdReq req)
        {
            var corporation = _repository.FindBy(req.Identify.CorporationKeyId);
            var dept = corporation.Departments.FirstOrDefault(c => c.KeyId == req.KeyId);
            if (dept != null)
            {
                dept.IsDelete = true;
            }
            _repository.Commit();
            return Task.FromResult(new OperateResultRsp
            {
                OperateFlag = true,
                OperateResult = string.Empty
            });
        }


        #endregion

        #region 角色
        public Task<OperateResultRsp> CreateRole(RoleEditReq req)
        {
            OperateResultRsp rsp = new OperateResultRsp();
            var corporation = _repository.FindBy(req.Identify.CorporationKeyId);
            if (corporation != null)
            {
                if (!corporation.CorpRoles.Exists(a => a.Name == req.RoleName.Trim()))
                {
                    var corpRole = new CorpRole
                    {
                        Name = req.RoleName,
                        CorporationKeyId = req.Identify.CorporationKeyId,
                      
                    };
                    corpRole.SetEditer(null);
                    corpRole.KeyId = Guid.NewGuid();
                    corporation.CorpRoles.Add(corpRole);
                    _repository.Commit();
                    rsp.OperateFlag = true;
                }
                else
                {
                    rsp.OperateFlag = false;
                    rsp.FlagErrorMsg = "角色名字已经存在";
                }
            }
            else
            {
                rsp.OperateFlag = false;
                rsp.FlagErrorMsg = "当前公司编号错误";
            }
            return Task.FromResult(rsp);
        }


        #endregion

        #region 员工
        public Task<OperateResultRsp> CreateEmployee(EmployeeEditReq req)
        {
            OperateResultRsp rsp = new OperateResultRsp();
            var corporation = _repository.FindBy(req.Identify.CorporationKeyId);
            if (corporation != null)
            {
                var  dept = corporation.Departments.FirstOrDefault(a => a.KeyId == req.DeptKeyId);
                if (dept != null)
                {
                    var emp = new Employee
                    {
                        Name = req.Name,
                        CorporationKeyId = req.Identify.CorporationKeyId,
                        DepartmentKeyId=req.DeptKeyId,
                        RoleKeyId = req.RoleKeyId,
                        RoleName = req.RoleName

                    };
                    emp.SetEditer(null);
                    emp.KeyId = Guid.NewGuid();
                    dept.Employees.Add(emp);
                    _repository.Commit();
                    rsp.OperateFlag = true;
                }
                else
                {
                    rsp.OperateFlag = false;
                    rsp.FlagErrorMsg = "角色名字已经存在";
                }
            }
            else
            {
                rsp.OperateFlag = false;
                rsp.FlagErrorMsg = "当前公司编号错误";
            }
            return Task.FromResult(rsp);
        }

     
        #endregion
    }
}
