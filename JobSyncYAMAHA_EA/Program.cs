using System;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace JobSyncYAMAHA_EA
{
    internal class Program
    {
        public static string _Connection = ConfigurationSettings.AppSettings["ConnectionString"];
        static void Main(string[] args)
        {
            var _context = new YAMAHADataContext(_Connection);

            //var tran = _context.Transaction.Connection.BeginTransaction();
            try
            {
                var viewAll = _context.V_EMPLOYEE_TYMs.ToList();


                foreach(var viewEmp in viewAll)
                {
                    var positionQuery = _context.MSTPositions.Where(x => x.NameEn == viewEmp.NAMPOS || x.NameTh == viewEmp.NAMPOS);
                    if (!positionQuery.Any(x => x.NameEn == viewEmp.NAMPOS || x.NameTh == viewEmp.NAMPOS))
                    {
                        var position = new MSTPosition();
                        position.CreatedDate = DateTime.Now;
                        position.ModifiedDate = DateTime.Now;
                        position.IsActive = true;
                        position.NameEn = viewEmp.NAMPOS.Replace(Environment.NewLine, "").Trim();
                        position.NameTh = viewEmp.NAMPOS.Replace(Environment.NewLine, "").Trim();
                        position.CreatedBy = "SYSTEM";
                        position.ModifiedBy = "SYSTEM";
                        position.CompanyCode = "TYM";
                        _context.MSTPositions.InsertOnSubmit(position);
                    }

                    var deptQuery = _context.MSTDepartments.Where(x => x.NameEn == viewEmp.department_e || x.NameTh == viewEmp.department_t);
                    if(!deptQuery.Any(x => x.NameEn == viewEmp.department_e || x.NameTh == viewEmp.department_t))
                    {
                        var dept = new MSTDepartment();
                        dept.CreatedDate = DateTime.Now;
                        dept.ModifiedDate = DateTime.Now;
                        dept.IsActive = true;
                        dept.NameEn = viewEmp.department_e;
                        dept.NameTh = viewEmp.department_t;
                        dept.CreatedBy = "SYSTEM";
                        dept.ModifiedBy = "SYSTEM";
                        dept.CompanyCode = "TYM";
                        _context.MSTDepartments.InsertOnSubmit(dept);
                    }

                    var divQuery = _context.MSTDivisions.Where(x => x.NameEn == viewEmp.division_e || x.NameTh == viewEmp.division_t);
                    if (!deptQuery.Any(x => x.NameEn == viewEmp.department_e || x.NameTh == viewEmp.department_t))
                    {
                        var div = new MSTDivision();
                        div.CreatedDate = DateTime.Now;
                        div.ModifiedDate = DateTime.Now;
                        div.IsActive = true;
                        div.NameEn = viewEmp.department_e;
                        div.NameTh = viewEmp.department_t;
                        div.CreatedBy = "SYSTEM";
                        div.ModifiedBy = "SYSTEM";
                        _context.MSTDivisions.InsertOnSubmit(div);
                    }

                    _context.SubmitChanges();
                }

                var updates = _context.V_EMPLOYEE_TYMs.Where(x => _context.MSTEmployees.Select(s => s.EmployeeCode).Contains(x.CODEMPID)).ToList();

                var inserts = _context.V_EMPLOYEE_TYMs.Where(x => !_context.MSTEmployees.Select(s => s.EmployeeCode).Contains(x.CODEMPID)).ToList();

                Console.WriteLine($"EMP UPDATE COUNT : {updates.Count()}");
                Console.WriteLine($"EMP INSERT COUNT : {inserts.Count()}");

                var empUpdates = _context.MSTEmployees.Where(x => updates.Select(s => s.CODEMPID).Contains(x.EmployeeCode)).ToList();

                foreach (var update in empUpdates)
                {
                    var mapper = updates.FirstOrDefault(x => update.EmployeeCode == x.CODEMPID);
                    update.Username = !string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.CODEMPID : mapper.CODEMPID;
                    update.NameTh = mapper.NAMEMPT;
                    update.NameEn = mapper.NAMEMPE;
                    update.Email = mapper.EMAIL;
                    update.PositionId = _context.MSTPositions.FirstOrDefault(x => x.NameEn == mapper.NAMPOS || x.NameTh == mapper.NAMPOS)?.PositionId;
                    update.DepartmentId = _context.MSTDepartments.FirstOrDefault(x => x.NameEn == mapper.department_e)?.DepartmentId;
                    update.DivisionId = _context.MSTDivisions.FirstOrDefault(x => x.NameEn == mapper.division_e || x.NameTh == mapper.division_t)?.DivisionId;

                    update.ReportToEmpCode = _context.MSTEmployees.FirstOrDefault(x => 
                    x.EmployeeCode == (!string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.codeHead : mapper.codeHead))
                        ?.EmployeeId.ToString();

                    update.ModifiedBy = "SYSTEM";
                    update.ModifiedDate = DateTime.Now;

                    _context.SubmitChanges();
                }

                foreach (var mapper in inserts)
                {
                    var insertModel = new MSTEmployee();

                    insertModel.Username = !string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.CODEMPID : mapper.CODEMPID;
                    insertModel.EmployeeCode = mapper.CODEMPID;
                    insertModel.NameTh = mapper.NAMEMPT;
                    insertModel.NameEn = mapper.NAMEMPE;
                    insertModel.Email = mapper.EMAIL;
                    insertModel.PositionId = _context.MSTPositions.FirstOrDefault(x => x.NameEn == mapper.NAMPOS || x.NameTh == mapper.NAMPOS)?.PositionId;
                    insertModel.DepartmentId = _context.MSTDepartments.FirstOrDefault(x => x.NameEn == mapper.department_e)?.DepartmentId;
                    insertModel.DivisionId = _context.MSTDivisions.FirstOrDefault(x => x.NameEn == mapper.division_e || x.NameTh == mapper.division_t)?.DivisionId;

                    insertModel.ReportToEmpCode = _context.MSTEmployees.FirstOrDefault(x =>
                    x.EmployeeCode == (!string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.codeHead : mapper.codeHead))
                        ?.EmployeeId.ToString();

                    insertModel.IsActive = true;
                    insertModel.Lang = "EN";
                    insertModel.AccountId = 1;
                    insertModel.ADTitle = string.Empty;

                    insertModel.ModifiedBy = "SYSTEM";
                    insertModel.ModifiedDate = DateTime.Now;
                    insertModel.CreatedBy = "SYSTEM";
                    insertModel.CreatedDate = DateTime.Now;
                    _context.MSTEmployees.InsertOnSubmit(insertModel);
                    _context.SubmitChanges();
                }
                //tran.Commit();
            }

            catch (Exception ex)
            {
                //tran.Rollback();
                Console.WriteLine(ex.ToString());
                Thread.Sleep(100000);
            }
        }
    }
}
