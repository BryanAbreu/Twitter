using AutoMapper;
using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitt_prof.ViewModels;

namespace Twitt_prof.Infraestructure.AutoMapper
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            ConfigureUsuario();
            ConfigurePost();
            ConfigureAmigos();
        }
        private void ConfigureAmigos()
        {
            CreateMap<AmigoViewModel, Amigos>().ReverseMap();
        }
        private void ConfigurePost()
        {
            CreateMap<PostViewModel, UserPost>().ReverseMap().ForMember(dest => dest.foto, opt => opt.Ignore()); ;
        }

        private  void ConfigureUsuario()
        {
            CreateMap<RegisterViewModel, Usuario>().ReverseMap().ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore());
        }
    }
}
