using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfApp6
{
    class ColorInfo : INotifyPropertyChanged
    {
        private string name;
        private Color rgb;
        private Color inverseRgb;
        public string Name
        {
            get { return name; }
            set
            {
                if(name != value)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        public Color Rgb
        {
            get { return rgb; }
            set
            {
                if (rgb != value)
                {
                    rgb = value;
                    NotifyPropertyChanged("Rgb");
                }
            }
        }
        public Color InverseRgb
        {
            get { return inverseRgb; }
            set
            {
                if (inverseRgb != value)
                {
                    inverseRgb = value;
                    NotifyPropertyChanged("InverseRgb");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string name)
        {
            if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
