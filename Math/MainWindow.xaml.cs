using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Math.Annotations;

namespace Math
{
    


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {


        public MainWindow()
        {
            InitializeComponent();

            CubeSizeSlider.Value = CubeRenderer.CubeMult;
            CubeOpacitySlider.Value = CubeRenderer.CubeOpacity;
            UpdateFrequencySlider.Value = CubeRenderer.RuntimeFrequency;
            ShadowBlurSlider.Value = CubeRenderer.ShadowBlurRadius;
            EnableShadowsCb.IsChecked = CubeRenderer.DrawShadows;
            EnableColorsCb.IsChecked = CubeRenderer.ColorCube;
            ShadowQualityCoB.SelectedIndex = CubeRenderer.ShadowQuality;
            BoxLineThickness.Value = CubeRenderer.CubeLineThickness;
            CubeRotationSpeed.Value = CubeRenderer.CubeRotationSpeed;
        }

        private void EnableShadowsChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox check)
            {
                if (check.IsChecked != null && check.IsChecked.Value)
                    CubeRenderer.DrawShadows = true;
                else if (check.IsChecked != null && !check.IsChecked.Value)
                    CubeRenderer.DrawShadows = false;

            }
        }

        private void EnableColorsChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox check)
            {
                if (check.IsChecked != null && check.IsChecked.Value)
                    CubeRenderer.ColorCube = true;
                else if (check.IsChecked != null && !check.IsChecked.Value)
                    CubeRenderer.ColorCube = false;

            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox CBox)
            {
                switch (CBox.SelectedIndex)
                {
                    case 0:
                        CubeRenderer.ShadowQuality = 0;
                        break;
                    case 1:
                        CubeRenderer.ShadowQuality = 1;
                        break;
                    default:
                        break;
                }
            }
        }

        private void CubeSizeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                CubeRenderer.CubeMult = slider.Value;
            }
        }


        private void CubeOpacitySliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                CubeRenderer.CubeOpacity = slider.Value;
            }
        }

        private void UpdateFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                CubeRenderer.RuntimeFrequency = (int) slider.Value;
            }
        }

        private void ShadowBlurChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                CubeRenderer.ShadowBlurRadius = slider.Value;
            }
        }

        private void BoxLineThicknessChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                CubeRenderer.CubeLineThickness = slider.Value;
            }
        }

        private void BoxSpinSpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                CubeRenderer.CubeRotationSpeed = slider.Value;
            }
        }
    }
}
