using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAObfuscator
{
    public class MVVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private string missionFolder;
        public string MissionFolder
        {
            get { return missionFolder; }
            set
            {
                if (Equals(value, missionFolder)) return;
                missionFolder = value;
                RaisePropertyChanged("MissionFolder");
            }
        }

        private string exportFolder;
        public string ExportFolder
        {
            get { return exportFolder; }
            set
            {
                if (Equals(value, exportFolder)) return;
                exportFolder = value;
                RaisePropertyChanged("ExportFolder");
            }
        }
        private string makepboFolder;
        public string MakePboFolder
        {
            get { return makepboFolder; }
            set
            {
                if (Equals(value, makepboFolder)) return;
                makepboFolder = value;
                RaisePropertyChanged("MakePboFolder");
            }
        }

        private string appVersion;
        public string AppVersion
        {
            get { return appVersion; }
            set
            {
                if (Equals(value, appVersion)) return;
                appVersion = value;
                RaisePropertyChanged("AppVersion");
            }
        }

        private bool makePbo;
        public bool MakePbo
        {
            get { return makePbo; }
            set
            {
                if (Equals(value, makePbo)) return;
                makePbo = value;
                RaisePropertyChanged("MakePbo");
            }
        }

        #region Interfaces
        public string this[string columnName] => null;

        public string Error => string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
