﻿using InterventionManagementSystem_MVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InterventionManagementSystem_MVC.Areas.SiteEngineer.Models
{

    public class SiteEngineerViewModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string DistrictName { get; set; }

        public Nullable<decimal> AuthorisedHours { get; set; }

        public Nullable<decimal> AuthorisedCosts { get; set; }
    }

    public class SiteEngineerViewInterventionModel
    {
        //[Key]
        //public int Id { get; set; }
       // public int SelectedType { get; set; }
       [Required]
        public string SelectedClient { get; set; }
        [Required]
        public string SelectedType { get; set; }
        public IEnumerable<InterventionViewModel> Interventions { get; set; }
        
        public IEnumerable<SelectListItem> ViewInterventionTypeList { get; set; }
        
        public IEnumerable<SelectListItem> ViewClientsList { get; set; }

        public InterventionViewModel Intervention { get; set; }
    }

    public class SiteEngineerViewClientModel
    {
        public InterventionViewModel Intervention { get; set; }

        public ClientViewModel Client { get; set; }

        public IEnumerable<InterventionViewModel> Interventions { get; set; }
    }
}