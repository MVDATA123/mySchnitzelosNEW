using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Controllers.ViewModels.Store
{
    public class StoreCreateViewModel
    {
        [Display(Name = "Filialname")]
        public string Name { get; set; }
        [Display(Name = "Stadt")]
        public string City { get; set; }
        [Display(Name = "Straße")]
        public string Street { get; set; }
        [Display(Name = "Hausnummer")]
        public string HouseNr { get; set; }
        [Display(Name = "Plz")]
        public string Plz { get; set; }
        [DisplayName("Tags")]
        public IList<string> SelectedTags { get; set; }
        [DisplayName("Filial-Banner")]
        public String ImageData { get; set; }
        [DisplayName("Firma")]
        public Guid CompanyId { get; set; }
        [DisplayName("Staat")]
        public Guid CountryId { get; set; }
    }
}