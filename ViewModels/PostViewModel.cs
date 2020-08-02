using Database.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Twitt_prof.ViewModels
{
    public class PostViewModel
    {
        public string NombreUsuario { get; set; }
        public string Post { get; set; }

        public int IdPost { get; set; }
        public string fotopost { get; set; }

        [Display(Name = "foto")]
        public IFormFile foto { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; }

    }
}
