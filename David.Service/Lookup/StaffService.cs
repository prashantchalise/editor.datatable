/*
 Service For Staffs
Created by: Prashant
Created On: 09/06/2019
*/
using David.Model;
using David.Model.ChaliseStoredProc;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;

namespace David.Service
{
    public interface IStaffsService : IEntityService<Staffs>
    {
        Staffs GetStaffsById(int staffid);
        IEnumerable<StaffsViewModel> GetStaffs(string userId, StaffsViewModel_Input input);
        void UpdateStaffs(Staffs staffs, string actionType, string userId, ref int returnStaffId, ref string msgType, ref string msgText);
    }

    public class StaffsService : EntityService<Staffs>, IStaffsService
    {

        new IContext _context;
        public StaffsService(IContext context) : base(context)
        {
            _context = context;
            _dbset = _context.Set<Staffs>();
        }

        //Stored Procedures definition 
        StoredProc csp_Staffs_GET = new StoredProc().HasName("[csp.Staffs.Get]").ReturnsTypes(typeof(StaffsViewModel));
        StoredProc csp_Staffs_UPDATE = new StoredProc().HasName("[csp.Staffs.Update]");

        /// <summary>
        /// Get Staffs By Id :: Don't forget to add the DBSet to RolpoContext
        /// </summary>

        public Staffs GetStaffsById(int staffid)
        {
            return _dbset.FirstOrDefault(x => x.StaffId == staffid);
        }

        /// <summary>
        /// Get Staffs
        /// </summary>

        public IEnumerable<StaffsViewModel> GetStaffs(string userId, StaffsViewModel_Input input)
        {

            SqlParameter[] p = new SqlParameter[6];

            p[0] = new SqlParameter("@StaffId", input.StaffId);
            p[1] = new SqlParameter("@first_name", input.first_name);

            p[2] = new SqlParameter("@UserId", userId);
            p[3] = new SqlParameter("@PageNumber", input.PageNumber);
            p[4] = new SqlParameter("@PageSize", input.PageSize);
            p[5] = new SqlParameter("@ShowAll", input.ShowAll);

            var results = _context.CallSP(csp_Staffs_GET, p);
            return results.ToList<StaffsViewModel>();
        }

        /// <summary>
        /// Update Staffs
        /// </summary>

        public void UpdateStaffs(Staffs staffs, string actionType, string userId, ref int returnStaffId, ref string msgType, ref string msgText)
        {

            SqlParameter[] p = new SqlParameter[10];

            p[0] = new SqlParameter("@ActionType", actionType);

            p[1] = new SqlParameter("@StaffId", staffs.StaffId);
            p[2] = new SqlParameter("@first_name", staffs.first_name);
            p[3] = new SqlParameter("@last_name", staffs.last_name);
            p[4] = new SqlParameter("@position", staffs.position);
            p[5] = new SqlParameter("@salary", staffs.salary);

            p[6] = new SqlParameter("@UserId", userId);

            p[7] = new SqlParameter("@MsgType", System.Data.SqlDbType.VarChar, 20);
            p[7].Direction = System.Data.ParameterDirection.Output;

            p[8] = new SqlParameter("@MsgText", System.Data.SqlDbType.VarChar, 200);
            p[8].Direction = System.Data.ParameterDirection.Output;

            p[9] = new SqlParameter("@ReturnStaffsId", System.Data.SqlDbType.Int);
            p[9].Direction = System.Data.ParameterDirection.Output;

            var result = _context.CallSP(csp_Staffs_UPDATE, p);

            msgType = (string)p[7].Value;
            msgText = (string)p[8].Value;
            returnStaffId = (int)p[9].Value;
        }
    }
}
