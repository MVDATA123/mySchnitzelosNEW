using System;
using System.Collections.Generic;
using System.Linq;
using GCloud.Shared.Dto.Domain;

namespace GCloudiPhone.Extensions
{
    public class StoreLocationDto : StoreDto
    {
        private readonly StoreDto _storeDto;
        public string Address => $"{Street} {HouseNr.Split('/').ToList().FirstOrDefault() ?? ""}, {Plz} {City}";
        public double? DistanceToUser { get; set; }


        public Guid Id
        {
            get => _storeDto.Id;
            set => _storeDto.Id = value;
        }

        public string Name
        {
            get => _storeDto.Name;
            set => _storeDto.Name = value;
        }

        public string City
        {
            get => _storeDto.City;
            set => _storeDto.City = value;
        }

        public string Street
        {
            get => _storeDto.Street;
            set => _storeDto.Street = value;
        }

        public string HouseNr
        {
            get => _storeDto.HouseNr;
            set => _storeDto.HouseNr = value;
        }

        public string Plz
        {
            get => _storeDto.Plz;
            set => _storeDto.Plz = value;
        }

        public DateTime CreationDateTime
        {
            get => _storeDto.CreationDateTime;
            set => _storeDto.CreationDateTime = value;
        }

        public CompanyDto Company
        {
            get => _storeDto.Company;
            set => _storeDto.Company = value;
        }

        public CountryDto Country
        {
            get => _storeDto.Country;
            set => _storeDto.Country = value;
        }

        public ICollection<TagDto> Tags
        {
            get => _storeDto.Tags;
            set => _storeDto.Tags = value;
        }

        public bool HasTag(string tag)
        {
            return Tags?.FirstOrDefault(t => t.Name.ToLower().Contains(tag.ToLower())) != null;
        }

        public StoreLocationDto(StoreDto storeDto)
        {
            _storeDto = storeDto;
        }

        public StoreLocationDto()
        {
            _storeDto = new StoreDto();
        }

        public double Latitude
        {
            get => _storeDto.Latitude;
            set => _storeDto.Latitude = value;
        }

        public double Longitude
        {
            get => _storeDto.Longitude;
            set => _storeDto.Longitude = value;
        }

        public bool IsUserFollowing
        {
            get => _storeDto.IsUserFollowing;
            set => _storeDto.IsUserFollowing = value;
        }

        public new string BannerImage
        {
            get => _storeDto.BannerImage;
            set => _storeDto.BannerImage = value;
        }
    }
}