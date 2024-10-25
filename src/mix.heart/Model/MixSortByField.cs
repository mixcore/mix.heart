using System.ComponentModel.DataAnnotations;
using Mix.Heart.Enums;
namespace Mix.Heart.Model
{
    public class MixSortByField
    {
        public MixSortByField(string fieldName, SortDirection sortDirection)
        {
            FieldName = fieldName;
            Direction = sortDirection;
        }

        [Required]
        public string FieldName { get; set; }

        public SortDirection Direction { get; set; }
    }
}
