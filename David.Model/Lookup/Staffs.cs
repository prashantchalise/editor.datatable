/* 
Model for [dbo].[Staffs] 
Created by: Prashant 
Created On: 09/06/2019 
 */
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace David.Model
{
    [Table("TBL_Staffs")]
    public class Staffs : AuditableEntity<long>
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StaffId { get; set; }


        [Required]
        [MaxLength(100)]
        [Display(Name = "first_name")]
        public string first_name { get; set; }


        [Required]
        [MaxLength(100)]
        [Display(Name = "last_name")]
        public string last_name { get; set; }


        [MaxLength(50)]
        [Display(Name = "position")]
        public string position { get; set; }


        [Display(Name = "salary")]
        public decimal salary { get; set; }

    }

    /* Staffs View Model */
    public class StaffsViewModel
    {
        public Int64 RowNumber { get; set; }
        public int StaffId { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string position { get; set; }
        public decimal salary { get; set; }
        public string ContactsJSON { get; set; }
        public int TotalCount { get; set; }
    }

    /* Staffs View Model (Input) */
    public class StaffsViewModel_Input
    {
        public int? StaffId { get; set; }
        public string first_name { get; set; }
         public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? ShowAll { get; set; }
    }
}
