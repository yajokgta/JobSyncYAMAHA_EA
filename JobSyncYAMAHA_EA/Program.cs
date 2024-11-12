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
                        position.IsActive = true;
                        position.NameEn = viewEmp.NAMPOSE.Replace(Environment.NewLine, "").Trim();
                        position.NameTh = viewEmp.NAMPOST.Replace(Environment.NewLine, "").Trim();
                        position.CreatedBy = "SYSTEM";
                        position.ModifiedBy = "SYSTEM";
                        position.CompanyCode = "TYM";
                        _context.MSTPositions.InsertOnSubmit(position);
                    }

                    var deptQuery = _context.MSTDepartments.Where(x => x.NameEn == viewEmp.NAMCENTENG || x.NameTh == viewEmp.NAMCENTTHA);
                    if (!deptQuery.Any(x => x.NameEn == viewEmp.NAMCENTENG || x.NameTh == viewEmp.NAMCENTTHA))
                    {
                        var dept = new MSTDepartment();
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

                //var inserts = _context.V_EMPLOYEE_TYMs.Where(x => !_context.MSTEmployees.Select(s => s.EmployeeCode).Contains(x.CODEMPID)).ToList();

                Console.WriteLine($"EMP UPDATE COUNT : {updates.Count()}");
                //Console.WriteLine($"EMP INSERT COUNT : {inserts.Count()}");

                var empUpdates = _context.MSTEmployees.Where(x => updates.Select(s => s.CODEMPID).Contains(x.EmployeeCode)).ToList();

                foreach (var update in empUpdates)
                {
                    var mapper = updates.FirstOrDefault(x => update.EmployeeCode == x.CODEMPID);
                    update.Username = !string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.CODEMPID : mapper.CODEMPID;
                    update.NameTh = mapper.NAMEMPT;
                    update.NameEn = mapper.NAMEMPE;
                    update.Email = mapper.EMAIL;
                    update.PositionId = _context.MSTPositions.FirstOrDefault(x => x.NameEn == mapper.NAMPOSE || x.NameTh == mapper.NAMPOST)?.PositionId;
                    update.DepartmentId = _context.MSTDepartments.FirstOrDefault(x => x.NameEn == mapper.NAMCENTENG || x.NameTh == mapper.NAMCENTENG)?.DepartmentId;
                    update.DivisionId = _context.MSTDivisions.FirstOrDefault(x => x.NameEn == mapper.department_e || x.NameTh == mapper.department_t)?.DivisionId;

                    //update.ReportToEmpCode = _context.MSTEmployees.FirstOrDefault(x => 
                    //x.EmployeeCode == (!string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.codeHead : mapper.codeHead))
                    //    ?.EmployeeId.ToString();
                    update.ReportToEmpCode = _context.MSTEmployees.Where(x => x.EmployeeCode == mapper.codeHead && x.IsActive == true).FirstOrDefault()?.EmployeeId.ToString() ?? null;

                    update.ModifiedBy = "SYSTEM";
                    update.ModifiedDate = DateTime.Now;

                    _context.SubmitChanges();
                }

                //foreach (var mapper in inserts)
                //{
                //    var insertModel = new MSTEmployee();

                //    insertModel.Username = !string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.CODEMPID : mapper.CODEMPID;
                //    insertModel.EmployeeCode = mapper.CODEMPID;
                //    insertModel.NameTh = mapper.NAMEMPT;
                //    insertModel.NameEn = mapper.NAMEMPE;
                //    insertModel.Email = mapper.EMAIL;
                //    insertModel.PositionId = _context.MSTPositions.FirstOrDefault(x => x.NameEn == mapper.NAMPOS || x.NameTh == mapper.NAMPOS)?.PositionId;
                //    insertModel.DepartmentId = _context.MSTDepartments.FirstOrDefault(x => x.NameEn == mapper.NAMCENTENG || x.NameTh == mapper.NAMCENTHA)?.DepartmentId;
                //    insertModel.DivisionId = _context.MSTDivisions.FirstOrDefault(x => x.NameEn == mapper.department_e || x.NameTh == mapper.department_t)?.DivisionId;

                //    //insertModel.ReportToEmpCode = _context.MSTEmployees.FirstOrDefault(x =>
                //    //x.EmployeeCode == (!string.IsNullOrEmpty(mapper.CODNATNL) && mapper.CODNATNL == "01" ? "CN" + mapper.codeHead : mapper.codeHead))
                //    //    ?.EmployeeId.ToString();
                //    insertModel.ReportToEmpCode = _context.MSTEmployees.Where(x => x.EmployeeCode == mapper.codeHead && x.IsActive == true).FirstOrDefault()?.EmployeeId.ToString() ?? null;

                //    insertModel.IsActive = true;
                //    insertModel.Lang = "EN";
                //    insertModel.AccountId = 1;
                //    insertModel.ADTitle = string.Empty;

                //    insertModel.ModifiedBy = "SYSTEM";
                //    insertModel.ModifiedDate = DateTime.Now;
                //    insertModel.CreatedBy = "SYSTEM";
                //    insertModel.CreatedDate = DateTime.Now;
                //    _context.MSTEmployees.InsertOnSubmit(insertModel);
                //    _context.SubmitChanges();
                //}
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
