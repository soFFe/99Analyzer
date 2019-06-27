using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NinetyNineLibrary;
using NinetyNineLibrary.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NinetyNineAnalyzer_WebApp.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class IndexModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public void OnGet()
        {
            
        }

        public async Task<ActionResult> OnPost()
        {
            var parsed = await NinetyNineParser.Parse(Input.TeamUrl);
            var retVal = new OutputModel
            {
                Success = !ErrorHandling.HasErrors
            };

            if(retVal.Success)
            {
                retVal.Team = parsed.Keys.First();
                retVal.Matches = parsed[retVal.Team];
            }
            else
            {
                retVal.Errors = ErrorHandling.JoinedString;
                ErrorHandling.Clear();
            }
            
            return new ObjectResult(retVal);
        }
    }

    public class InputModel
    {
        public string TeamUrl { get; set; }
    }

    public class OutputModel
    {
        public bool Success { get; set; }
        public string Errors { get; set; }
        public Team Team { get; set; }
        public List<Match> Matches { get; set; }
    }
}
