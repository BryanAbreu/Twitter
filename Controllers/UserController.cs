using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Database.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.AccessControl;
using Email;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Twitt_prof.ViewModels;
using Repository.Repository;

namespace Twitt_prof.Controllers
{
    public class UserController : Controller
    {
        private readonly TwittContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly PostRepository _postRepository;

        public UserController(TwittContext context, IEmailSender emailSender,
            IMapper mapper, IWebHostEnvironment hostEnvironment,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            PostRepository postRepository)
        {
            _emailSender = emailSender;
            _context = context;
            this._mapper = mapper;
            this.hostEnvironment = hostEnvironment;
            _signInManager = signInManager;
            _userManager = userManager;
            _postRepository = postRepository;

        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("TwitHome", "User");
            }
            var user = User.Identity.Name;
            if (!string.IsNullOrEmpty(user))
            {
                return RedirectToAction("TwitHome", "User");

            }
            return View();
        }
        public IActionResult Post()
        {
            return View();
        }
        //get post amigos

        //get:post
        public IActionResult TwitHome(UsuarioViewModel usuarioViewModel)
        {
           var userSession = User.Identity.Name;
            var ListadoPost = _context.UserPost.Where(x => x.NombreUsuario.Contains(userSession)).ToList();
            List<PostViewModel> vm = new List<PostViewModel>();

            ListadoPost.ForEach(item =>
            {
                vm.Add(new PostViewModel
                {
                    Post = item.Post,
                    NombreUsuario = item.NombreUsuario,
                    fotopost = item.Foto
                });
            });
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (ModelState.IsValid)
            {

                var resul = await _signInManager.PasswordSignInAsync(vm.NombreUsuario, vm.Password, false, false);
                if (resul.Succeeded)
                {
                    return RedirectToAction("TwitHome", "User");

                }
                ModelState.AddModelError("UserOrPasswordInvalid", "el usuario o contraseña son incorrectos");
            }
            return View(vm);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }
        public IActionResult Register()
        {
            
            return View();
        }
        public async Task<IActionResult> Usuarios()
        {
            var Listadoamigos = await _context.Users.ToListAsync();
            List<UsuarioViewModel> vm = new List<UsuarioViewModel>();
            Listadoamigos.ForEach(item =>
            {
                vm.Add(new UsuarioViewModel
                {
                    NombreUsuario = item.UserName

                });
            });
            return View(vm);
        }
      [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = vm.NombreUsuario, Email = vm.Email };
                var result = await _userManager.CreateAsync(user, vm.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("TwitHome", "User");
                }
                AddErrors(result);

            }
            return View(vm);
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        [HttpPost]
        public async Task<IActionResult> TwitHome(PostViewModel postViewModel)
        {
            if (ModelState.IsValid)
            {
                string uniqueName = null;
                if (postViewModel.foto != null)
                {
                    var folderPath = Path.Combine(hostEnvironment.WebRootPath, "images/post");
                    uniqueName = Guid.NewGuid().ToString() + "_" + postViewModel.foto.FileName;
                    var filePath = Path.Combine(folderPath, uniqueName);
                    if (filePath != null)
                    {
                        var stream = new FileStream(filePath, mode: FileMode.Create);
                        postViewModel.foto.CopyTo(stream);
                        stream.Flush();
                        stream.Close();
                    }
                }
               
                var post = _mapper.Map<UserPost>(postViewModel);
                post.Foto = uniqueName;
                post.NombreUsuario= User.Identity.Name;
                await _postRepository.Add(post);
            }
            return RedirectToAction(nameof(TwitHome));
        }
        [HttpPost]
        public IActionResult Usuarioss(string amigoss, string NombreUsuario)
        {
            var userSession = User.Identity.Name;
            if (!string.IsNullOrEmpty(userSession))
            {
                var ListadoAmigos = _context.Users.Where(x => x.UserName.Contains(amigoss)).ToList();
                return View(ListadoAmigos);
            }
            return View();
        }
       [HttpPost]
        public async Task<IActionResult> AñadirAmigo(AmigoViewModel amigos, UsuarioViewModel usuarioViewModel)
        {
            if (ModelState.IsValid)
            {
                var am = new Amigos
                {
                    NombreUsuario = User.Identity.Name,
                    NombreAmigo = usuarioViewModel.NombreUsuario
                };
                _context.Add(am);
                await _context.SaveChangesAsync();

            }
            return RedirectToAction(nameof(TwitHome));
        }
        
        public async Task<IActionResult> eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            
            }
            var post = await _postRepository.GetById(id.Value);

            if (post == null)
            {
                return NotFound();
            }
            var vm = new PostViewModel
            {
                IdPost = post.IdPost,
                Post = post.Post,
                fotopost = post.Foto
            };


            return View(vm); 
        }


        [HttpPost, ActionName("borrar")]
        public async Task<IActionResult> borrar(int id)
        {
          await  _postRepository.Delete(id);
            return RedirectToAction("TwitHome", "User");

        }
    }




}
