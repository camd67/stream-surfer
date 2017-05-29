
using System.ComponentModel.DataAnnotations;

namespace StreamSurfer.Models
{
    public enum ShowRating
    {
        [Display(Name ="Not Rated")]
        NOT_RATED = 0,
        [Display(Name = "1")]
        ONE = 1,
        [Display(Name = "2")]
        TWO = 2,
        [Display(Name = "3")]
        THREE = 3,
        [Display(Name = "4")]
        FOUR = 4,
        [Display(Name = "5")]
        FIVE = 5
    }
}
