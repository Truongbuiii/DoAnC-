using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FoodTourApp.Extensions
{
    public static class PageExtensions
    {
        public static Task DisplayAlertAsync(this Page page, string title, string message, string cancel)
            => page.DisplayAlert(title, message, cancel);
    }
}
