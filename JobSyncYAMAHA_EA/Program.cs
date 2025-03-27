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


                foreach (var viewEmp in viewAll)
                {
                    var positionQuery = _context.MSTPositions.Where(x => x.NameEn == viewEmp.NAMPOSE || x.NameTh == viewEmp.NAMPOST);
                    if (!positionQuery.Any(x => x.NameEn == viewEmp.NAMPOSE || x.NameTh == viewEmp.NAMPOST))
                    {
                        var position = new MSTPosition();
                        position.CreatedDate = DateTime.Now;
                        position.ModifiedDate = DateTime.Now;
                        position.AccountId = 1;
                        position.IsActive = true;
                        position.NameEn = (viewEmp.NAMPOSE ?? "").Replace(Environment.NewLine, "").Trim();
                        position.NameTh = (viewEmp.NAMPOST ?? "").Replace(Environment.NewLine, "").Trim();
                        position.CreatedBy = "SYSTEM";
                        position.ModifiedBy = "SYSTEM";
                        position.CompanyCode = "TYM";
                        _context.MSTPositions.InsertOnSubmit(position);
                    }

                    var deptQuery = _context.MSTDepartments.Where(x => x.NameEn == viewEmp.NAMCENTENG || x.NameTh == viewEmp.NAMCENTTHA);
                    if (!deptQuery.Any(x => x.NameEn == viewEmp.NAMCENTENG || x.NameTh == viewEmp.NAMCENTTHA))
                    {
                        var dept = new MSTDepartment();
                        dept.AccountId = 1;
                        dept.CreatedDate = DateTime.Now;
                        dept.ModifiedDate = DateTime.Now;
                        dept.IsActive = true;
                        dept.NameEn = viewEmp.NAMCENTENG;
                        dept.NameTh = viewEmp.NAMCENTTHA;
                        dept.CreatedBy = "SYSTEM";
                        dept.ModifiedBy = "SYSTEM";
                        dept.CompanyCode = "TYM";
                        dept.DepartmentCode = !string.IsNullOrEmpty(viewEmp.CODCOMP) ? viewEmp.CODCOMP : null;
                        _context.MSTDepartments.InsertOnSubmit(dept);
                    }

                    var divQuery = _context.MSTDivisions.Where(x => x.NameEn == viewEmp.department_e || x.NameTh == viewEmp.department_t);
                    if (!divQuery.Any(x => x.NameEn == viewEmp.department_e || x.NameTh == viewEmp.department_t))
                    {
                        var div = new MSTDivision();
                        div.AccountId = 1;
                        div.DivisionId = 0;
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

                var updates = _context.V_EMPLOYEE_TYMs.ToList().Select(ss => new
                {
                    CODEMPID = !string.IsNullOrEmpty(ss.CODNATNL) && ss.CODNATNL == "01" ? "CN" + ss.CODEMPID : ss.CODEMPID,
                    ss.CODNATNL,
                    ss.NAMEMPT,
                    ss.NAMEMPE,
                    ss.EMAIL,
                    ss.NAMPOSE,
                    ss.NAMCENTENG,
                    ss.NAMPOST,
                    ss.department_e,
                    ss.department_t,
                    ss.codeHead
                }).Where(x => _context.MSTEmployees.Select(s => s.EmployeeCode).Contains(x.CODEMPID)).ToList();

                Console.WriteLine($"EMP UPDATE COUNT : {updates.Count()}");
                var usersCode = updates.Select(s => s.CODEMPID);
                var empUpdates = _context.MSTEmployees.Where(x => usersCode.Contains(x.EmployeeCode)).ToList();
                var empIsActive = _context.MSTEmployees.Where(x => !usersCode.Contains(x.EmployeeCode)).ToList();

                foreach (var update in empUpdates)
                {
                    var mapper = updates.FirstOrDefault(x => update.EmployeeCode == x.CODEMPID);
                    update.Username = mapper.CODEMPID;
                    update.EmployeeCode = mapper.CODEMPID;
                    update.NameTh = mapper.NAMEMPT;
                    update.NameEn = mapper.NAMEMPE;
                    update.Email = mapper.EMAIL;
                    update.PositionId = _context.MSTPositions.FirstOrDefault(x => x.NameEn == mapper.NAMPOSE || x.NameTh == mapper.NAMPOST)?.PositionId;
                    update.DepartmentId = _context.MSTDepartments.FirstOrDefault(x => x.NameEn == mapper.NAMCENTENG || x.NameTh == mapper.NAMCENTENG)?.DepartmentId;
                    update.DivisionId = _context.MSTDivisions.FirstOrDefault(x => x.NameEn == mapper.department_e || x.NameTh == mapper.department_t)?.DivisionId;

                    update.ReportToEmpCode = _context.MSTEmployees.Where(x => x.EmployeeCode == (!string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.codeHead : mapper.codeHead)).FirstOrDefault()?.EmployeeId.ToString() ?? null;

                    update.ModifiedBy = "SYSTEM";
                    update.ModifiedDate = DateTime.Now;

                    _context.SubmitChanges();
                }

                foreach (var update in empIsActive)
                {
                    update.IsActive = false;
                    _context.SubmitChanges();
                }
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
