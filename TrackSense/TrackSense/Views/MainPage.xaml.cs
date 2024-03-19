using System.ComponentModel;
using TrackSense.ViewModels;

namespace TrackSense.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }
    }
}