using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Models
{
    public class TodoItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string AssignedTo { get; set; }
        [Required]
        public string TenantId { get; set; }
    }
}
