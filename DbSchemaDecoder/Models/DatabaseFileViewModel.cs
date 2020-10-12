using CommonDialogs.Common;
using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DbSchemaDecoder.Models
{
    public class DatabaseFileViewModel : NotifyPropertyChangedImpl
    {
        public DataBaseFile DataBaseFile { get; set; }

        string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(ErrorMessage); }
        }

        Color _backgroundColour = Colors.Black;
        public Color Color
        {
            get
            {
                return _backgroundColour;
            }
            set
            {
                _backgroundColour = value;
                NotifyPropertyChanged();
            }
        }
    }
}
