using System.ComponentModel.DataAnnotations;

namespace QuanLyChiTieu.ViewModels
{
    public class CreateExpenseJarViewModel
    {
        public long JarId { get; set; }

        [Required(ErrorMessage = "Tên hũ không được để trống")]
        public string JarName { get; set; } = null!;
    }


}
