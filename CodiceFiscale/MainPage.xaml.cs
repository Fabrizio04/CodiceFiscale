using CodiceFiscale.ViewModels;

namespace CodiceFiscale
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnComuneEntryUnfocused(object sender, FocusEventArgs e)
        {
            // Un piccolo delay è necessario perché se l'utente clicca 
            // su un suggerimento, l'evento Unfocused scatta PRIMA del Tap.
            await Task.Delay(200);

            if (BindingContext is MainViewModel vm)
            {
                vm.IsSuggerimentiVisible = false;
            }
        }
    }
}
