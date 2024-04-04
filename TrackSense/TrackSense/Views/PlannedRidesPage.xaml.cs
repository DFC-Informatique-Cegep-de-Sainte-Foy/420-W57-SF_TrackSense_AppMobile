using System.ComponentModel;
using TrackSense.ViewModels;

namespace TrackSense.Views
{
    public partial class PlannedRidesPage : ContentPage
    {
        readonly Animation animation;
        public PlannedRidesPage(PlannedRidesViewModel viewModel)
        {
            InitializeComponent();
            /*
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            animation = new Animation(v => receptionImg.TranslationX = v,
                -40, screenWidth, Easing.SinIn);

            BindingContext = viewModel;

            viewModel.PropertyChanged += viewModel_PropertyChanged;*/
        }
        
        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*
            if (BindingContext is PlannedRidesViewModel viewModel)
            {
                if (e.PropertyName == nameof(viewModel.IsReceivingData))
                {
                    if (viewModel.IsReceivingData)
                    {
                        animation.Commit(this, "animate", 16, 2500, Easing.SinIn,
                            (v, c) => receptionImg.TranslationX = -40, () => true);
                    }
                    else
                    {
                        this.AbortAnimation("animate");
                    }
                }
            }*/
        }
        
    }
}