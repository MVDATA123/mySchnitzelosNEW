using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GCloud.Shared.Dto.Domain;
using Java.Util;
using Optional;

namespace mvdata.foodjet.Domain
{
    public class StoreLocationDto : StoreDto
    {
        private readonly StoreDto _storeDto;
        private LatLng _storeLocation;

        public string Address
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Street) || string.IsNullOrWhiteSpace(Plz) || string.IsNullOrWhiteSpace(City))
                {
                    return null;
                }

                return $"{Street ?? ""} {HouseNr?.Split('/').ToList().FirstOrDefault() ?? ""}, {Plz ?? ""} {City ?? ""}";
            }
        }

        public LatLng StoreLocation
        {
            get
            {
                if (_storeLocation != null)
                {
                    return _storeLocation;
                }
                _storeLocation = new LatLng(_storeDto.Latitude, _storeDto.Longitude);
                return _storeLocation;
            }
            set => _storeLocation = value;
        }

        public double Longitude
        {
            get => StoreLocation?.Longitude ?? 0;
            set {
                if (_storeLocation == null)
                {
                    _storeLocation = new LatLng(0,value);
                }
                else
                {
                    _storeLocation.Longitude = value;
                }
            }
        }

        public double Latitude
        {
            get => StoreLocation?.Latitude ?? 0;
            set
            {
                if (_storeLocation == null)
                {
                    _storeLocation = new LatLng(value,0);
                }
                else
                {
                    _storeLocation.Latitude = value;
                }
            }
        }
        public new Guid Id
        {
            get => _storeDto.Id;
            set => _storeDto.Id = value;
        }

        public new string Name
        {
            get => _storeDto.Name;
            set => _storeDto.Name = value;
        }

        public new string City
        {
            get => _storeDto.City;
            set => _storeDto.City = value;
        }

        public new string Street
        {
            get => _storeDto.Street;
            set => _storeDto.Street = value;
        }

        public new string HouseNr
        {
            get => _storeDto.HouseNr;
            set => _storeDto.HouseNr = value;
        }

        public new string Plz
        {
            get => _storeDto.Plz;
            set => _storeDto.Plz = value;
        }

        public new DateTime CreationDateTime
        {
            get => _storeDto.CreationDateTime;
            set => _storeDto.CreationDateTime = value;
        }

        public new CompanyDto Company
        {
            get => _storeDto.Company;
            set => _storeDto.Company = value;
        }

        public new CountryDto Country
        {
            get => _storeDto.Country;
            set => _storeDto.Country = value;
        }

        public StoreLocationDto()
        {
            _storeDto = new StoreDto();
        }

        public StoreLocationDto(StoreDto storeDto)
        {
            _storeDto = storeDto;
            StoreLocation = new LatLng(Latitude, Longitude);
        }

        public ICollection<TagDto> Tags
        {
            get => _storeDto.Tags;
            set => _storeDto.Tags = value;
        }

        public bool IsUserFollowing
        {
            get => _storeDto.IsUserFollowing;
            set => _storeDto.IsUserFollowing = value;
        }

        public string BannerImage
        {
            get => _storeDto.BannerImage;
            set => _storeDto.BannerImage = value;
        }

        public bool HasTag(string tag)
        {
            return Tags?.Any(t => t.Name.ToUpper().Contains(tag.ToUpper())) ?? false;
        }
    }
}