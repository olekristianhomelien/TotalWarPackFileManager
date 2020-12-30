using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace VariantMeshEditor.ViewModels.Animation.AnimationSplicer.BoneMapping
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class IsRootNodeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is AdvBoneMappingBone))
                return null;
            return ((AdvBoneMappingBone)value).ParentBoneIndex == -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(bool), typeof(BoneMappingType))]
    public class HasBoneMappingClourConverter : IValueConverter
    {
        SolidColorBrush TrueValue { get; set; }
        SolidColorBrush FalseValue { get; set; }

        public HasBoneMappingClourConverter()
        {
            // set defaults
            TrueValue = new SolidColorBrush(Colors.Black); ;
            FalseValue = new SolidColorBrush(Colors.Red);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is BoneMappingType))
                return FalseValue;

            var setting = ((BoneMappingType)value);
            if (setting == BoneMappingType.None)
                return FalseValue;

            return TrueValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}