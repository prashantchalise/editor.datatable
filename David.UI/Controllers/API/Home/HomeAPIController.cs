using Microsoft.AspNet.Identity;
using David.UI.Models;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Xml.Serialization;
using David.Service;
using System.Collections.Generic;
using David.Model;
using System.Linq;
using System;
using System.Web.Http.ModelBinding;

namespace David.UI.Controllers.API
{
    [RoutePrefix("api/Home")]
	public class HomeAPIController : ApiController
    {
        IStaffsService _staffsService;


        /// <summary>
        /// Constructor with DI
        /// </summary>
        /// 
        public HomeAPIController(
            IStaffsService staffsService
            )
		{
            _staffsService = staffsService;
        }


        #region "STAFFS"  

        /// <summary>
        /// Action: Get List of Staffss
        /// </summary>

        [HttpPost, Route("Staffs/GetStaffssList")]
        public IEnumerable<StaffsViewModel> GetStaffssList([FromBody] StaffsViewModel_Input input)
        {
            var user = GetCurrentUser();
             return _staffsService.GetStaffs(userId: user.Id
                , input: input);

        }

        /// <summary>
        /// Action: Get Staffs by Id
        /// </summary>

        [HttpGet, Route("Staffs/GetStaffsById")]
        public StaffsViewModel GetStaffsById([FromUri] int staffid)
        {
            var user = GetCurrentUser();

            var input = new StaffsViewModel_Input() { StaffId = staffid, PageNumber = 1, PageSize = 1, ShowAll = 0 };

            var result = _staffsService.GetStaffs(
            userId: user.Id, input: input).FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Action: Add Staffs
        /// </summary>

        [HttpPost, Route("Staffs/SaveStaffs")]
        public IHttpActionResult SaveStaffs([FromBody] Staffs staffs)
        {
            string msgType = "", msgText = "", actionType = "ADD";
            int StaffId = 0;
            var user = GetCurrentUser();

            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }

            try
            {
                string userId = user.Id;
                //Set OrganizationId  
                _staffsService.UpdateStaffs(staffs: staffs, actionType: actionType, userId: userId, msgType: ref msgType, msgText: ref msgText, returnStaffId: ref StaffId);
            }
            catch (Exception e)
            {
                var modelStateDictionary = new ModelStateDictionary();
                if (e.InnerException != null)
                    modelStateDictionary.AddModelError("InnerException", e.InnerException.Message);
                else
                    modelStateDictionary.AddModelError("InnerException", e.Message);
                return BadRequest(modelStateDictionary);
            }

            if (msgType == "ERROR")
            {
                var modelStateDictionary = new ModelStateDictionary();
                modelStateDictionary.AddModelError("InnerException", msgText);
                return BadRequest(modelStateDictionary);
            }

            var input = new StaffsViewModel_Input() { StaffId = StaffId, PageNumber = 1, PageSize = 1, ShowAll = 0 };

           
            return Ok(_staffsService.GetStaffs(
            userId: user.Id, input: input).FirstOrDefault());
        }

        /// <summary>
        /// Action: Update Staffs
        /// </summary>

        [HttpPost, Route("Staffs/UpdateStaffs")]
        public IHttpActionResult UpdateStaffs([FromBody] Staffs staffs)
        {
            string msgType = "", msgText = "", actionType = "UPDATE";
            int StaffId = 0;
            var user = GetCurrentUser();

            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }

            try
            {
                string userId = user.Id;
                _staffsService.UpdateStaffs(staffs: staffs, actionType: actionType, userId: userId, msgType: ref msgType, msgText: ref msgText, returnStaffId: ref StaffId);
            }
            catch (Exception e)
            {
                var modelStateDictionary = new ModelStateDictionary();
                if (e.InnerException != null)
                    modelStateDictionary.AddModelError("InnerException", e.InnerException.Message);
                else
                    modelStateDictionary.AddModelError("InnerException", e.Message);
                return BadRequest(modelStateDictionary);
            }

            if (msgType == "ERROR")
            {
                var modelStateDictionary = new ModelStateDictionary();
                modelStateDictionary.AddModelError("InnerException", msgText);
                return BadRequest(modelStateDictionary);
            }

            var input = new StaffsViewModel_Input() { StaffId = StaffId, PageNumber = 1, PageSize = 1, ShowAll = 0 };

            return Ok(_staffsService.GetStaffs(
            userId: user.Id, input: input).FirstOrDefault());
        }

        /// <summary>
        /// Action: Delete Staffs
        /// </summary>

        [HttpDelete, Route("Staffs/DeleteStaffs/{staffid}")]
        public IHttpActionResult DeleteStaffs(int staffid)
        {
            string msgType = "", msgText = "", actionType = "DELETE";
            int StaffId = 0;
            var user = GetCurrentUser();
            StaffsViewModel staffsVM = GetStaffsById(staffid: staffid);
            if (staffsVM == null)
            { return NotFound(); }
            try
            {
                Staffs staffs = new Staffs()
                {
                    StaffId = staffsVM.StaffId
                };
                string userId = user.Id;
                _staffsService.UpdateStaffs(staffs: staffs, actionType: actionType, userId: userId, msgType: ref msgType, msgText: ref msgText, returnStaffId: ref StaffId);
            }
            catch (Exception e)
            {
                var modelStateDictionary = new ModelStateDictionary();
                if (e.InnerException != null)
                    modelStateDictionary.AddModelError("InnerException", e.InnerException.Message);
                else
                    modelStateDictionary.AddModelError("InnerException", e.Message);
                return BadRequest(modelStateDictionary);
            }

            return Ok();
        }

        #endregion
        #region Helper Functions  

        // Get Current User
        private static ApplicationUser GetCurrentUser()
		{
			ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
			return user;
		}

		// Add Errors
		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty,error);
			}
		}

		// To XML Function
		public static string ToXML(object dataToSerialize)
		{
			var stringwriter = new System.IO.StringWriter();
			var serializer = new XmlSerializer(dataToSerialize.GetType());
			serializer.Serialize(stringwriter, dataToSerialize);
			return stringwriter.ToString();
		}

		#endregion


	}
}
