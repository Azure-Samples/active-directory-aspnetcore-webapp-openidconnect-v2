using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_OpenIDConnect_DotNet.Models
{
    public class AuthorizedTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string TempAuthorizationCode { get; set; }
        public string TenantId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? AuthorizedOn { get; set; }
    }
}
