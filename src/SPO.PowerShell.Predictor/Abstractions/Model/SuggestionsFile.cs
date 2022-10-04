using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.PowerShell.Predictor.Abstractions.Model
{
    public class SuggestionsFile
    {
        public string? FileName { get; set; }
        public string? LastUpdatedOn { get; set; }
        public List<Suggestion>? Suggestions { get; set; }
    }
}
