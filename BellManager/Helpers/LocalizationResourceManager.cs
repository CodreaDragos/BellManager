using System.ComponentModel;
using System.Globalization;
using BellManager.Resources.Strings;

namespace BellManager.Helpers
{
    public class LocalizationResourceManager : INotifyPropertyChanged
    {
        private static readonly Lazy<LocalizationResourceManager> currentHolder = new(() => new LocalizationResourceManager());

        public static LocalizationResourceManager Instance => currentHolder.Value;
        
        private const string LanguagePreferenceKey = "AppLanguage";

        private LocalizationResourceManager()
        {
            // Load saved language preference or use current culture
            var savedLanguage = Preferences.Get(LanguagePreferenceKey, string.Empty);
            
            if (!string.IsNullOrEmpty(savedLanguage))
            {
                try
                {
                    AppResources.Culture = new CultureInfo(savedLanguage);
                }
                catch
                {
                    AppResources.Culture = CultureInfo.CurrentCulture;
                }
            }
            else
            {
                AppResources.Culture = CultureInfo.CurrentCulture;
            }
        }

        public object this[string resourceKey]
            => AppResources.ResourceManager.GetObject(resourceKey, AppResources.Culture) ?? Array.Empty<byte>();

        public void SetCulture(CultureInfo culture)
        {
            AppResources.Culture = culture;
            
            // Save the language preference
            Preferences.Set(LanguagePreferenceKey, culture.Name);
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
