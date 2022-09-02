using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AutoMapper;
using GCloud.Shared.Dto.Domain;
using mvdata.foodjet.Filter;

namespace mvdata.foodjet.Mapper
{
    public class AutoMapperConfig
    {
        private static AutoMapperConfig _instace;
        public IMapper Mapper { get; }
        public static AutoMapperConfig Instance => _instace ?? (_instace = new AutoMapperConfig());

        private AutoMapperConfig()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TagDto, TagViewModel>());
            Mapper = config.CreateMapper();
        }
    }
}