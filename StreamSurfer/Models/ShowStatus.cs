using System.ComponentModel.DataAnnotations;

namespace StreamSurfer.Models
{
    public enum ShowStatus
    {
        [Display(Name = "Watching")]
        WATCHING = 0,
        [Display(Name = "Complete")]
        COMPLETED = 1,
        [Display(Name = "Want to Watch")]
        WANT_TO_WATCH = 2
    }
}
