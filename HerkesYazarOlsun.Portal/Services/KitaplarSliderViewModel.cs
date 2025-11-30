using HerkesYazarOlsun.Model.ViewModel;

namespace HerkesYazarOlsun.Portal.Services
{
    // Slider için kullanılacak ViewModel
    public class KitaplarSliderViewModel
    {
        public List<VM_BOOKS> VMBooksList { get; set; } = new List<VM_BOOKS>();

        public string Tip { get; set; }  // modeldeki Tip
        public bool? isAnaSayfa { get; set; }  // modeldeki isAnaSayfa
    }

}
