using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using AudioView.Common.Data;

namespace AudioView.Web.Models
{
    public class MeasurementsSearchResultModel
    {
        [Display(Name = "Project Name")]
        public string ProjectName;
        
        [Display(Name = "From")]
        public string From;
        
        [Display(Name = "To")]
        public string To;
        
        public IList<Project> Projects;
    }

    public class ReadingsModel
    {
        public IList<Reading> Readings { get; set; }
        public Project Project { get; set; }
        public List<Reading> MajorReadings { get; set; }
        public List<Reading> MinorReadings { get; set; }
    }
}