using System.ComponentModel;
using TrackSense.ViewModels;

namespace TrackSense.Views
{
    public partial class CreateNewPlannedRidePage : ContentPage
    {
        readonly Animation animation;
        public CreateNewPlannedRidePage()
        {
            InitializeComponent();
            /*
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            animation = new Animation(v => receptionImg.TranslationX = v,
                -40, screenWidth, Easing.SinIn);
            
            BindingContext = viewModel;

            viewModel.PropertyChanged += viewModel_PropertyChanged;*/
        }

        /*
        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BindingContext is CreateNewPlannedRideViewModel viewModel)
            {
                if (e.PropertyName == nameof(viewModel.IsConnected))
                {
                    if (viewModel.IsConnected)
                    {
                        animation.Commit(this, "animate", 16, 2500, Easing.SinIn,
                            (v, c) => receptionImg.TranslationX = -40, () => true);
                    }
                    else
                    {
                        this.AbortAnimation("animate");
                    }
                }
            }
        }*/
        
    }
}